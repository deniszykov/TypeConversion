using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
// ReSharper disable InconsistentNaming

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Class providing <see cref="IConverter"/> and <see cref="IConverter{FromType,ToType}"/> instances on demand.
	/// </summary>
	[PublicAPI]
	public partial class TypeConversionProvider : ITypeConversionProvider, ICustomConversionRegistry
	{
		private static readonly int ConverterArrayIncrementCount = 5;

		public const string IgnoreCaseFormat = "ignoreCase";

		private static class ConversionLookupIndex
		{
			private static int LastFromIndex = -1;
			private static int LastEnumIndex = -1;

			// ReSharper disable StaticMemberInGenericType, UnusedTypeParameter
			public static class FromType<FromT>
			{
				private static int LastToIndex = -1;

				public static class ToType<ToT>
				{
					public static readonly int ToIndex = Interlocked.Increment(ref LastToIndex);
				}

				public static readonly int FromIndex = Interlocked.Increment(ref LastFromIndex);
			}
			public static class EnumType<FromT>
			{
				public static readonly int EnumIndex = Interlocked.Increment(ref LastEnumIndex);
			}
			// ReSharper restore StaticMemberInGenericType, UnusedTypeParameter

		}
		private enum KnownNativeConversion
		{
			Unknown = 0,
			EnumToNumber,
			EnumToEnum,
			NumberToEnum,
			EnumToString,
			StringToEnum,
			NullableToNullable,
			NullableToAny,
			AnyToNullable,
			UpCasting,
			DownCasting,
		}
#if !NETSTANDARD
		private class TypeConverterAdapter<FromTypeT, ToTypeT>
		{
			private readonly System.ComponentModel.TypeConverter typeConverter;

			public TypeConverterAdapter(System.ComponentModel.TypeConverter typeConverter)
			{
				if (typeConverter == null) throw new ArgumentNullException(nameof(typeConverter));

				this.typeConverter = typeConverter;
			}

			public ToTypeT ConvertTo(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
			{
				return (ToTypeT)this.typeConverter.ConvertTo(null, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, fromValue, typeof(ToTypeT))!;
			}
			public ToTypeT ConvertFrom(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				return (ToTypeT)this.typeConverter.ConvertFrom(null!, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, fromValue);
			}
		}
#endif

		private IConverter?[]?[] converters;
		private IEnumConversionInfo?[] enumConversionInfos;
		private readonly MethodInfo getConverterDefinition;
		private readonly ConcurrentDictionary<long, Func<IConverter>> getConverterByTypes;
		private readonly IConversionMetadataProvider metadataProvider;
		private readonly IFormatProvider defaultFormatProvider;
		private readonly ConversionOptions converterOptions;
		private readonly ConversionMethodSelectionStrategy conversionMethodSelectionStrategy;

#pragma warning disable 1572
		/// <summary>
		/// Constructor of <see cref="TypeConversionProvider"/>.
		/// </summary>
		/// <param name="configuration">Configuration options.</param>
		/// <param name="configurationOptions">Configuration options.</param>
		/// <param name="metadataProvider">Metadata provider used to discover conversion method on types. If null then instance of <see cref="ConversionMetadataProvider"/> is created.</param>
		/// <param name="registrations">Registrations of custom providers. Alternative way of registering custom conversion is <see cref="TypeConversionProviderOptions"/>.</param>
#pragma warning restore 1572
		public TypeConversionProvider(
#if NET45
			TypeConversionProviderOptions? configuration = null,
#else
			Microsoft.Extensions.Options.IOptions<TypeConversionProviderOptions>? configurationOptions = null,

#endif
			IConversionMetadataProvider? metadataProvider = null,
			IEnumerable<ICustomConversionRegistration>? registrations = null
		)
		{
#if !NET45
			var configuration = configurationOptions?.Value;
#endif
			this.converters = new IConverter[ConverterArrayIncrementCount][];
			this.enumConversionInfos = new IEnumConversionInfo[ConverterArrayIncrementCount];
			this.getConverterByTypes = new ConcurrentDictionary<long, Func<IConverter>>();
			this.getConverterDefinition = new Func<IConverter>(this.GetConverter<object, object>).GetMethodInfo()!.GetGenericMethodDefinition();
			this.metadataProvider = metadataProvider ?? new ConversionMetadataProvider();
			this.defaultFormatProvider = configuration?.DefaultFormatProvider ?? CultureInfo.InvariantCulture;
			if (string.IsNullOrEmpty(configuration?.DefaultFormatProviderCultureName) == false)
			{
#if NETSTANDARD1_6
				this.defaultFormatProvider = new CultureInfo(configuration!.DefaultFormatProviderCultureName!);
#else
				this.defaultFormatProvider = CultureInfo.GetCultureInfo(configuration!.DefaultFormatProviderCultureName!);
#endif
			}
			this.converterOptions = configuration?.Options ?? ConversionOptions.Default;
			this.conversionMethodSelectionStrategy = configuration?.ConversionMethodSelectionStrategy ?? ConversionMethodSelectionStrategy.MostFittingMethod;

			this.InitializeNativeConversions();
			this.InitializeCustomConversion();

#pragma warning disable 618
			configuration?.CustomConversionRegistrationCallback?.Invoke(this);
#pragma warning restore 618

			if (registrations != null)
			{
				foreach (var registration in registrations)
				{
					registration.Register(this);
				}
			}
		}

		/// <inheritdoc />
		public IConverter<FromTypeT, ToTypeT> GetConverter<FromTypeT, ToTypeT>()
		{
			var fromTypeIndex = ConversionLookupIndex.FromType<FromTypeT>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromTypeT>.ToType<ToTypeT>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			if (this.GetType().Name == string.Empty)
			{
				// AOT static compilation hack
				PrepareTypesForAotRuntime<FromTypeT, ToTypeT>();
			}

			if (toConverters[toTypeIndex] is IConverter<FromTypeT, ToTypeT> converter)
			{
				return converter;
			}
			else
			{
				var conversionDescriptor = this.CreateConversionDescriptor<FromTypeT, ToTypeT>();
				converter = new Converter<FromTypeT, ToTypeT>(this, conversionDescriptor, this.converterOptions);
				toConverters[toTypeIndex] = converter;
				return converter;
			}
		}

		/// <inheritdoc />
		public IConverter GetConverter(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var fromHash = fromType.GetHashCode(); // it's not hashcode, it's an unique sync-lock of type-object
			var toHash = toType.GetHashCode();
			var typePairIndex = unchecked(((long)fromHash << 32) | (uint)toHash);
			if (this.getConverterByTypes.TryGetValue(typePairIndex, out var getConverterFunc))
			{
				return getConverterFunc();
			}

			var getConverterMethod = this.getConverterDefinition.MakeGenericMethod(fromType, toType);
			getConverterFunc = ReflectionExtensions.CreateDelegate<Func<IConverter>>(this, getConverterMethod, throwOnBindFailure: true) ??
				throw new InvalidOperationException($"Failed to create conversion delegate from '{getConverterMethod}' method.");

			this.getConverterByTypes[typePairIndex] = getConverterFunc;

			return getConverterFunc();
		}

		/// <inheritdoc />
		public void RegisterConversion<FromTypeT, ToTypeT>(Func<FromTypeT, string, IFormatProvider, ToTypeT> conversionFunc, ConversionQuality quality)
		{
			var conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFunc, quality) };
			var fromTypeIndex = ConversionLookupIndex.FromType<FromTypeT>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromTypeT>.ToType<ToTypeT>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			var conversionDescriptor = new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(conversionMethods), null, this.defaultFormatProvider, conversionFunc, default);
			var converter = new Converter<FromTypeT, ToTypeT>(this, conversionDescriptor, this.converterOptions);
			toConverters[toTypeIndex] = converter;
		}

		private IConverter[] GetToConverters(int fromTypeIndex, int toTypeIndex)
		{
			if (fromTypeIndex >= this.converters.Length)
			{
				Array.Resize(ref this.converters, fromTypeIndex + ConverterArrayIncrementCount);
			}

			var toConverters = this.converters[fromTypeIndex];
			while (toConverters == null)
			{
				toConverters = Interlocked.CompareExchange(ref this.converters[fromTypeIndex], new IConverter?[ConverterArrayIncrementCount], null);
			}

			while (toTypeIndex >= toConverters!.Length)
			{
				var originalToConverters = toConverters;
				Array.Resize(ref toConverters, toConverters.Length + ConverterArrayIncrementCount);
				toConverters = Interlocked.CompareExchange(ref this.converters[fromTypeIndex], toConverters, originalToConverters);
			}

			return toConverters!;
		}

		private ConversionDescriptor CreateConversionDescriptor<FromTypeT, ToTypeT>()
		{
			var fromType = typeof(FromTypeT);
			var toType = typeof(ToTypeT);

			var conversionMethods = default(ConversionMethodInfo[]); // from best to worst
			var conversionFn = default(Func<FromTypeT, string?, IFormatProvider?, ToTypeT>);
			var safeConversionFn = default(Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>);
			var defaultFormat = default(string);
			var defaultFormatProvider = this.defaultFormatProvider;

			// fallback conversion is used when no conversion method is found on types
			var fallbackConversionFn = default(Func<FromTypeT, string?, IFormatProvider?, ToTypeT>?);
			var safeFallbackConversionFn = default(Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>?);
			var fallbackConversionMethodInfo = default(ConversionMethodInfo);

			switch (this.DetectNativeConversion(fromType, toType))
			{
				case KnownNativeConversion.UpCasting:
					conversionFn = UpCast<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.DownCasting:
					fallbackConversionFn = this.DownCast<FromTypeT, ToTypeT>;
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					safeFallbackConversionFn = this.TryDownCast<FromTypeT, ToTypeT>;
					goto default;
				case KnownNativeConversion.NullableToNullable:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = this.NullableToNullableAot<FromTypeT, ToTypeT>;
						safeFallbackConversionFn = this.TryNullableToNullableAot<FromTypeT, ToTypeT>;
					}
					else
					{
						var nullableToNullableMethod = InstantiateGenericMethod<int?, int?>(this.NullableToNullable<int, int>, Nullable.GetUnderlyingType(fromType)!, Nullable.GetUnderlyingType(toType)!);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, ToTypeT>>(this, nullableToNullableMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create nullable-to-nullable conversion delegate from '{nullableToNullableMethod}' method.");

						var safeNullableToNullableMethod = InstantiateGenericMethod<int?, int?>(this.TryNullableToNullable<int, int>, Nullable.GetUnderlyingType(fromType)!, Nullable.GetUnderlyingType(toType)!);
						safeFallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>>(this, safeNullableToNullableMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create safe nullable-to-nullable conversion delegate from '{safeNullableToNullableMethod}' method.");
					}

					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.NullableToAny:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = this.NullableToAnyAot<FromTypeT, ToTypeT>;
						safeFallbackConversionFn = this.TryNullableToAnyAot<FromTypeT, ToTypeT>;
					}
					else
					{
						var nullableToAnyMethod = InstantiateGenericMethod<int?, int>(this.NullableToAny<int, int>, Nullable.GetUnderlyingType(fromType)!, toType);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, ToTypeT>>(this, nullableToAnyMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create nullable-to-any conversion delegate from '{nullableToAnyMethod}' method.");

						var safeNullableToAnyMethod = InstantiateGenericMethod<int?, int>(this.TryNullableToAny<int, int>, Nullable.GetUnderlyingType(fromType)!, toType);
						safeFallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>>(this, safeNullableToAnyMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create safe nullable-to-any conversion delegate from '{safeNullableToAnyMethod}' method.");
					}
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.AnyToNullable:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = this.AnyToNullableAot<FromTypeT, ToTypeT>;
						safeFallbackConversionFn = this.TryAnyToNullableAot<FromTypeT, ToTypeT>;
					}
					else
					{
						var anyToNullableMethod = InstantiateGenericMethod<int, int?>(this.AnyToNullable<int, int>, fromType, Nullable.GetUnderlyingType(toType)!);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, ToTypeT>>(this, anyToNullableMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create any-to-nullable conversion delegate from '{anyToNullableMethod}' method.");

						var safeNullableToAnyMethod = InstantiateGenericMethod<int, int?>(this.TryAnyToNullable<int, int>, fromType, Nullable.GetUnderlyingType(toType)!);
						safeFallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>>(this, safeNullableToAnyMethod, throwOnBindFailure: true) ??
							throw new InvalidOperationException($"Failed to create safe any-to-nullable conversion delegate from '{safeNullableToAnyMethod}' method.");
					}
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.EnumToNumber:
					conversionFn = this.ConvertEnumToNumber<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.EnumToEnum:
					conversionFn = this.ConvertEnumToEnum<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.NumberToEnum:
					conversionFn = this.ConvertNumberToEnum<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.EnumToString:
					conversionFn = this.ConvertEnumToString<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.StringToEnum:
					conversionFn = this.ConvertStringToEnum<FromTypeT, ToTypeT>;
					safeConversionFn = this.TryConvertStringToEnum<FromTypeT, ToTypeT>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.Unknown:
				default:
					conversionMethods = this.FindConversionBetweenTypes(fromType, toType);
					break;
			}

#if !NETSTANDARD
			if (conversionMethods.Length == 0 || conversionMethods[0].Quality < ConversionQuality.TypeConverter)
			{
				var toTypeConverter = this.metadataProvider.GetTypeConverter(toType);
				if (toTypeConverter is NullableConverter) // worse than nothing
				{
					toTypeConverter = null; 
				}
				
				if (toTypeConverter?.CanConvertFrom(fromType) ?? false)
				{
					var adapter = new TypeConverterAdapter<FromTypeT, ToTypeT>(toTypeConverter);
					conversionFn = adapter.ConvertFrom;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.TypeConverter) };
				}

				var fromTypeConverter = this.metadataProvider.GetTypeConverter(fromType);
				if (fromTypeConverter is NullableConverter) // worse than nothing
				{
					toTypeConverter = null; 
				}

				if (conversionMethods.Length == 0 && (fromTypeConverter?.CanConvertTo(toType) ?? false))
				{
					var adapter = new TypeConverterAdapter<FromTypeT, ToTypeT>(fromTypeConverter);
					conversionFn = adapter.ConvertTo;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.TypeConverter) };
				}
			}
#endif
			if (conversionMethods.Length == 0 && fallbackConversionMethodInfo != null)
			{
				conversionMethods = new[] { fallbackConversionMethodInfo };
				conversionFn = fallbackConversionFn;
				safeConversionFn = safeFallbackConversionFn;
			}

			if (conversionMethods.Length == 0)
			{
				conversionFn = ThrowNoConversionBetweenTypes<FromTypeT, ToTypeT>;
				conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.None) };
				safeConversionFn = (_, _, _) => new KeyValuePair<ToTypeT, bool>(default!, false);
			}
			else if (conversionFn == null)
			{
				defaultFormat = this.metadataProvider.GetDefaultFormat(conversionMethods[0]);
				if ((fromType == typeof(float) || fromType == typeof(double)) && toType == typeof(string))
				{
					defaultFormat ??= "R";
				}
				else if ((fromType == typeof(DateTime) && toType == typeof(string)) ||
					(fromType == typeof(string) && toType == typeof(DateTime)) ||
					(fromType == typeof(DateTimeOffset) && toType == typeof(string)) ||
					(fromType == typeof(string) && toType == typeof(DateTimeOffset)))
				{
					defaultFormat ??= "o";
				}
				else if ((fromType == typeof(TimeSpan) && toType == typeof(string)) ||
					(fromType == typeof(string) && toType == typeof(TimeSpan)))
				{
					defaultFormat ??= "c";
				}

				if ((this.converterOptions & ConversionOptions.OptimizeWithExpressions) == 0)
				{
					conversionFn = this.PrepareConvertFunc<FromTypeT, ToTypeT>(conversionMethods);
				}
				else
				{
					conversionFn = this.PrepareConvertExpression<FromTypeT, ToTypeT>(conversionMethods);
				}
			}
			return new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(conversionMethods), defaultFormat, defaultFormatProvider, conversionFn, safeConversionFn);
		}

		private KnownNativeConversion DetectNativeConversion(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));


			var fromTypeInfo = fromType.GetTypeInfo();
			var toTypeInfo = toType.GetTypeInfo();
			var fromIsNullable = Nullable.GetUnderlyingType(fromType) != null;
			var toIsNullable = Nullable.GetUnderlyingType(toType) != null;

			if (this.metadataProvider.IsAssignableFrom(fromType, toType))
			{
				return KnownNativeConversion.UpCasting;
			}
			else if (fromIsNullable && toIsNullable)
			{
				return KnownNativeConversion.NullableToNullable;
			}
			else if (fromIsNullable)
			{
				return KnownNativeConversion.NullableToAny;
			}
			else if (toIsNullable)
			{
				return KnownNativeConversion.AnyToNullable;
			}
			else if (this.metadataProvider.IsAssignableFrom(toType, fromType))
			{
				return KnownNativeConversion.DownCasting;
			}
			else if (fromTypeInfo.IsEnum && toTypeInfo.IsEnum)
			{
				return KnownNativeConversion.EnumToEnum;
			}
			else if (fromTypeInfo.IsEnum && IsNumber(toType))
			{
				return KnownNativeConversion.EnumToNumber;
			}
			else if (toTypeInfo.IsEnum && IsNumber(fromType))
			{
				return KnownNativeConversion.NumberToEnum;
			}
			else if (fromTypeInfo.IsEnum && toType == typeof(string))
			{
				return KnownNativeConversion.EnumToString;
			}
			else if (toTypeInfo.IsEnum && fromType == typeof(string))
			{
				return KnownNativeConversion.StringToEnum;
			}
			else
			{
				return KnownNativeConversion.Unknown;
			}
		}

		private ConversionMethodInfo[] FindConversionBetweenTypes(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var conversionMethods = new List<ConversionMethodInfo>();

			foreach (var convertFromMethod in this.metadataProvider.GetConvertFromMethods(toType))
			{
				if (convertFromMethod.FromType == fromType && convertFromMethod.ToType == toType)
				{
					conversionMethods.Add(convertFromMethod);
				}
			}
			foreach (var convertFromMethod in this.metadataProvider.GetConvertToMethods(fromType))
			{
				if (convertFromMethod.FromType == fromType && convertFromMethod.ToType == toType)
				{
					conversionMethods.Add(convertFromMethod);
				}
			}

			// sort by quality

			conversionMethods.Sort(Comparer<ConversionMethodInfo>.Default);
			conversionMethods.Reverse();

			if (this.conversionMethodSelectionStrategy == ConversionMethodSelectionStrategy.MostSpecificMethod && conversionMethods.Count > 1)
			{
				conversionMethods.RemoveRange(1, conversionMethods.Count - 1);
			}

			// leave best method in each group, grouping by parameters count

			const int FORMAT_PARAM = 1;
			const int FORMAT_PROVIDER_PARAM = 2;

			// 4 groups:
			// 0x0: NONE,
			// 0x1: FORMAT_PARAM,
			// 0x2: FORMAT_PROVIDER_PARAM,
			// 0x3: FORMAT_PARAM & FORMAT_PROVIDER_PARAM
			var lastMethodQualityClass = -1;
			var lastReplaceIndex = 0;
			for (var m = 0; m < conversionMethods.Count; m++)
			{
				var conversionMethod = conversionMethods[m];
				var methodQualityClass = (conversionMethods[m].ConversionParameterTypes.IndexOf(ConversionParameterType.Format) >= 0 ? FORMAT_PARAM : 0) |
					(conversionMethods[m].ConversionParameterTypes.IndexOf(ConversionParameterType.FormatProvider) >= 0 ? FORMAT_PROVIDER_PARAM : 0);
				if (lastMethodQualityClass != methodQualityClass)
				{
					conversionMethods[lastReplaceIndex++] = conversionMethod;
					lastMethodQualityClass = methodQualityClass;
				}
			}
			conversionMethods.RemoveRange(lastReplaceIndex, conversionMethods.Count - lastReplaceIndex);

			//


			return conversionMethods.ToArray();
		}

		private void InitializeCustomConversion()
		{
			this.RegisterConversion<string, Uri>((value, format, _) =>
			{
				var kind = string.IsNullOrEmpty(format) ? UriKind.RelativeOrAbsolute : (UriKind)Enum.Parse(typeof(UriKind), format, ignoreCase: true);
				return new Uri(value, kind);
			}, ConversionQuality.Custom);

			this.RegisterConversion<Uri, string>((value, _, _) => value.OriginalString, ConversionQuality.Custom);

			this.RegisterConversion<string, DateTime>((str, f, fp) =>
			{
				if (f == null || string.Equals(f, "o", StringComparison.OrdinalIgnoreCase))
				{
					return DateTime.ParseExact(str, f ?? "o", fp, DateTimeStyles.RoundtripKind);
				}
				else
				{
					return DateTime.Parse(str, fp);
				}
			}, ConversionQuality.Custom);
		}

		private Func<FromTypeT, string?, IFormatProvider?, ToTypeT> PrepareConvertExpression<FromTypeT, ToTypeT>(params ConversionMethodInfo[] methods)
		{
			if (methods == null) throw new ArgumentNullException(nameof(methods));
			if (methods.Length == 0) throw new ArgumentOutOfRangeException(nameof(methods));

			var fromType = typeof(FromTypeT);
			var toType = typeof(ToTypeT);
			var fromValueParameter = Expression.Parameter(fromType, "fromValue");
			var formatParameter = Expression.Parameter(typeof(string), "format");
			var formatProviderParameter = Expression.Parameter(typeof(IFormatProvider), "formatProvider");

			var convertResultVariable = Expression.Parameter(toType, "result");
			var convertExpression = (Expression)Expression.Assign(
				convertResultVariable,
				CreateConvertExpression(methods.Last())
			);
			foreach (var method in methods.Reverse())
			{
				var hasFormatParameter = method.ConversionParameterTypes.IndexOf(ConversionParameterType.Format) >= 0;
				var hasFormatProviderParameter = method.ConversionParameterTypes.IndexOf(ConversionParameterType.FormatProvider) >= 0;

				var convertViaMethodExpression = CreateConvertExpression(method);
				if (!hasFormatParameter && !hasFormatProviderParameter)
				{
					convertExpression = Expression.Assign(
						convertResultVariable,
						convertViaMethodExpression
					);
				}
				else
				{
					var testExpression = default(Expression);
					if (hasFormatParameter && hasFormatProviderParameter)
					{
						testExpression = Expression.AndAlso(
							Expression.ReferenceNotEqual(formatParameter, Expression.Constant(null, formatParameter.Type)),
							Expression.ReferenceNotEqual(formatProviderParameter, Expression.Constant(null, formatProviderParameter.Type))
						);
					}
					else if (hasFormatParameter)
					{
						testExpression = Expression.ReferenceNotEqual(formatParameter, Expression.Constant(null, formatParameter.Type));
					}
					else
					{
						testExpression = Expression.ReferenceNotEqual(formatProviderParameter, Expression.Constant(null, formatProviderParameter.Type));
					}

					convertExpression = Expression.IfThenElse(
						test: testExpression,
						ifTrue: Expression.Assign(
							convertResultVariable,
							convertViaMethodExpression
						),
						ifFalse: convertExpression
					);
				}
			}

			Debug.Assert(convertExpression != null);

			return Expression.Lambda<Func<FromTypeT, string?, IFormatProvider?, ToTypeT>>(
				Expression.Block(new[] { convertResultVariable }, convertExpression, convertResultVariable),
				$"Convert_{fromType.Name}_{toType.Name}_via_{string.Join("_or_", methods.Select(m => $"{m.Method.Name}_{string.Join("_", m.Parameters.Select(p => p.Name))}"))}",
				new[] {
						fromValueParameter,
						formatParameter,
						formatProviderParameter
				}
			).Compile();

			Expression CreateConvertExpression(ConversionMethodInfo conversionMethodInfo)
			{
				var methodParameters = conversionMethodInfo.Parameters;
				var methodParameterTypes = conversionMethodInfo.ConversionParameterTypes;
				var arguments = new Expression[methodParameters.Count];

				if (formatParameter == null) throw new ArgumentNullException(nameof(formatParameter));
				if (formatProviderParameter == null) throw new ArgumentNullException(nameof(formatProviderParameter));
				if (fromValueParameter == null) throw new ArgumentNullException(nameof(fromValueParameter));

				for (var i = 0; i < methodParameters.Count; i++)
				{
					if (methodParameterTypes[i] == ConversionParameterType.Format)
					{
						arguments[i] = formatParameter;
					}
					else if (methodParameterTypes[i] == ConversionParameterType.FormatProvider)
					{
						arguments[i] = formatProviderParameter;
					}
					else if (methodParameters[i].ParameterType != fromValueParameter.Type)
					{
						arguments[i] = Expression.ConvertChecked(fromValueParameter, methodParameters[i].ParameterType);
					}
					else
					{
						arguments[i] = fromValueParameter;
					}
				}

				var convertCallExpression = default(Expression);
				if (conversionMethodInfo.Method is MethodInfo methodInfo)
				{
					if (methodInfo.IsStatic)
					{
						convertCallExpression = Expression.Call(methodInfo, arguments);
					}
					else
					{
						var callTarget = (Expression)fromValueParameter;
						if (callTarget.Type != methodInfo.DeclaringType && methodInfo.DeclaringType != null)
						{
							callTarget = Expression.ConvertChecked(callTarget, methodInfo.DeclaringType);
						}

						convertCallExpression = Expression.Call(callTarget, methodInfo, arguments);

						if (callTarget.Type.GetTypeInfo().IsValueType == false)
						{
							convertCallExpression = Expression.Condition(
								test: Expression.Equal(callTarget, Expression.Default(callTarget.Type)),
								ifTrue: Expression.Default(convertCallExpression.Type),
								ifFalse: convertCallExpression
							);
						}
					}
				}
				else if (conversionMethodInfo.Method is ConstructorInfo constructorInfo)
				{
					convertCallExpression = Expression.New(constructorInfo, arguments);
				}
				else
				{
					throw new InvalidOperationException($"Invalid conversion method: {conversionMethodInfo.Method}. " +
						$"This should be instance of '{typeof(MethodInfo)}' or '{typeof(ConstructorInfo)}'.");
				}

				if (convertCallExpression.Type != typeof(ToTypeT))
				{
					convertCallExpression = Expression.ConvertChecked(convertCallExpression, typeof(ToTypeT));
				}
				return convertCallExpression;
			}
		}
		private Func<FromTypeT, string?, IFormatProvider?, ToTypeT> PrepareConvertFunc<FromTypeT, ToTypeT>(params ConversionMethodInfo[] methods)
		{
			if (methods == null) throw new ArgumentNullException(nameof(methods));
			if (methods.Length == 0) throw new ArgumentOutOfRangeException(nameof(methods));

			const int FORMAT_PARAMETER = 0;
			const int FORMAT_PROVIDER_PARAMETER = 1;
			const int FROM_VALUE_PARAMETER = 2;

			var methodParametersMap = new int[methods.Length][];

			for (var m = 0; m < methods.Length; m++)
			{
				var parameterTypes = methods[m].ConversionParameterTypes;
				methodParametersMap[m] = new[] { -1, -1, -1 };

				for (var p = 0; p < parameterTypes.Count; p++)
				{
					if (parameterTypes[p] == ConversionParameterType.Format)
					{
						methodParametersMap[m][FORMAT_PARAMETER] = p;
					}
					else if (parameterTypes[p] == ConversionParameterType.FormatProvider)
					{
						methodParametersMap[m][FORMAT_PROVIDER_PARAMETER] = p;
					}
					else
					{
						methodParametersMap[m][FROM_VALUE_PARAMETER] = p;
					}
				}
			}

			return (fromValue, format, formatProvider) =>
			{
				for (var m = 0; m < methods.Length; m++)
				{
					var method = methods[m];
					var methodParameters = methods[m].Parameters;
					var fromValueParameterIndex = methodParametersMap[m][FROM_VALUE_PARAMETER];
					var formatParameterIndex = methodParametersMap[m][FORMAT_PARAMETER];
					var formatProviderParameterIndex = methodParametersMap[m][FORMAT_PROVIDER_PARAMETER];

					if ((formatParameterIndex >= 0 && format == null ||
						formatProviderParameterIndex >= 0 && formatProvider == null) &&
						m < methods.Length - 1) // is not last
					{
						continue; // fallback to method with less parameters
					}

					// prepare arguments
					var arguments = new object?[methodParameters.Count];
					if (fromValueParameterIndex >= 0)
					{
						arguments[fromValueParameterIndex] = fromValue;
					}
					if (formatParameterIndex >= 0)
					{
						arguments[formatParameterIndex] = format;
					}
					if (formatProviderParameterIndex >= 0)
					{
						arguments[formatProviderParameterIndex] = formatProvider;
					}

					// invoke method
					try
					{
						if (method.Method is MethodInfo methodInfo)
						{
							if (methodInfo.IsStatic)
							{
								return (ToTypeT)methodInfo.Invoke(null, arguments)!;
							}
							else if (ReferenceEquals(fromValue, null))
							{
								return default!;
							}
							else
							{
								return (ToTypeT)methodInfo.Invoke(fromValue, arguments)!;
							}
						}
						else if (method.Method is ConstructorInfo constructorInfo)
						{
							return (ToTypeT)constructorInfo.Invoke(arguments);
						}
						else
						{
							throw new InvalidOperationException($"Invalid conversion method: {method.Method}. This should be instance of '{typeof(MethodInfo)}' or '{typeof(ConstructorInfo)}'.");
						}
					}
					catch (TargetInvocationException invocationException)
					{
						throw invocationException.InnerException!.Rethrow();
					}
				}

				ThrowNoConversionBetweenTypes<FromTypeT, ToTypeT>(fromValue, format, formatProvider);
				throw new InvalidOperationException(); // never happens
			};
		}

		private EnumConversionInfo<EnumT> GetEnumConversionInfo<EnumT>()
		{
			var enumIndex = ConversionLookupIndex.EnumType<EnumT>.EnumIndex;
			var enumConversionInfos = this.enumConversionInfos;
			while (enumIndex >= enumConversionInfos.Length)
			{
				var originalEnumConversionInfos = enumConversionInfos;
				Array.Resize(ref enumConversionInfos, enumConversionInfos.Length + ConverterArrayIncrementCount);
				enumConversionInfos = Interlocked.CompareExchange(ref this.enumConversionInfos, enumConversionInfos, originalEnumConversionInfos);
			}

			var enumConversionInfo = (EnumConversionInfo<EnumT>?)this.enumConversionInfos[enumIndex];
			if (enumConversionInfo == null)
			{
				var useDynamicMethods = (this.converterOptions & ConversionOptions.OptimizeWithExpressions) != 0;
				Interlocked.Exchange(ref this.enumConversionInfos[enumIndex], enumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods));
			}
			return enumConversionInfo;
		}

		// predefined types transitions between nullable/enum/base-types/interfaces
		private ToTypeT NullableToNullableAot<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
			{
				return default!;
			}

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromTypeT)) ?? typeof(FromTypeT);
			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToTypeT)) ?? typeof(ToTypeT);

			var converter = this.GetConverter(fromUnderlyingType, toUnderlyingType);
			converter.Convert(fromValue, out var resultObj, format, formatProvider);
			return (ToTypeT)resultObj!;
		}
		private KeyValuePair<ToTypeT, bool> TryNullableToNullableAot<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
			{
				return new KeyValuePair<ToTypeT, bool>(default!, true);
			}

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromTypeT)) ?? typeof(FromTypeT);
			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToTypeT)) ?? typeof(ToTypeT);

			var converter = this.GetConverter(fromUnderlyingType, toUnderlyingType);
			if (converter.TryConvert(fromValue, out var result, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT, bool>((ToTypeT)result!, true);
			}
			else
			{
				return new KeyValuePair<ToTypeT, bool>(default!, false); // conversion failed
			}
		}
		private ToTypeT NullableToAnyAot<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var result = default(ToTypeT);
			if (ReferenceEquals(fromValue, null))
			{
				if (ReferenceEquals(result, null))
				{
					return default!;
				}
				else
				{
					throw new InvalidCastException($"Unable to convert <null> to '{typeof(ToTypeT)}'.");
				}
			}

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromTypeT)) ?? typeof(FromTypeT);
			var converter = this.GetConverter(fromUnderlyingType, typeof(ToTypeT));
			converter.Convert(fromValue, out var resultObj, format, formatProvider);
			return (ToTypeT)resultObj!;
		}
		private KeyValuePair<ToTypeT, bool> TryNullableToAnyAot<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
		{
			var result = default(ToTypeT);
			if (ReferenceEquals(fromValue, null))
			{
				if (ReferenceEquals(result, null))
				{
					return new KeyValuePair<ToTypeT, bool>(default!, true);
				}
				else
				{
					return new KeyValuePair<ToTypeT, bool>(default!, false); // unable to cast null to ToTypeT
				}
			}

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromTypeT)) ?? typeof(FromTypeT);
			var converter = this.GetConverter(fromUnderlyingType, typeof(ToTypeT));
			if (converter.TryConvert(fromValue, out var resultObj, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT, bool>((ToTypeT)resultObj!, true);

			}
			else
			{
				return new KeyValuePair<ToTypeT, bool>(default!, false);  // conversion failed
			}
		}
		private ToTypeT AnyToNullableAot<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
			{
				return default!;
			}

			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToTypeT)) ?? typeof(ToTypeT);
			var converter = this.GetConverter(typeof(FromTypeT), toUnderlyingType);
			converter.Convert(fromValue, out var result, format, formatProvider);
			return (ToTypeT)result!;
		}
		private KeyValuePair<ToTypeT, bool> TryAnyToNullableAot<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
			{
				return new KeyValuePair<ToTypeT, bool>(default!, true);
			}

			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToTypeT)) ?? typeof(ToTypeT);
			var converter = this.GetConverter(typeof(FromTypeT), toUnderlyingType);
			if (converter.TryConvert(fromValue, out var resultObj, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT, bool>((ToTypeT)resultObj!, true);
			}
			else
			{
				return new KeyValuePair<ToTypeT, bool>(default!, false);  // conversion failed
			}
		}
		private ToTypeT? NullableToNullable<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
			where ToTypeT : struct where FromTypeT : struct
		{
			if (fromValue.HasValue == false)
			{
				return default;
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue.Value, out var result, format, formatProvider);
			return result;
		}
		private KeyValuePair<ToTypeT?, bool> TryNullableToNullable<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
			where FromTypeT : struct where ToTypeT : struct
		{
			if (fromValue.HasValue == false)
			{
				return new KeyValuePair<ToTypeT?, bool>(default!, true);
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			if (converter.TryConvert(fromValue.Value, out var result, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT?, bool>(result, true);
			}
			else
			{
				return new KeyValuePair<ToTypeT?, bool>(default!, false);  // conversion failed
			}
		}
		private ToTypeT NullableToAny<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider) where FromTypeT : struct
		{
			var result = default(ToTypeT);
			if (fromValue.HasValue == false)
			{
				if (ReferenceEquals(result, null))
				{
					return default!;
				}
				else
				{
					throw new InvalidCastException($"Unable to convert <null> to '{typeof(ToTypeT)}'.");
				}
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue.GetValueOrDefault(), out result, format, formatProvider);
			return result!;
		}
		private KeyValuePair<ToTypeT, bool> TryNullableToAny<FromTypeT, ToTypeT>(FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
			where FromTypeT : struct
		{
			var result = default(ToTypeT);
			if (fromValue.HasValue == false)
			{
				if (ReferenceEquals(result, null))
				{
					return new KeyValuePair<ToTypeT, bool>(default!, true);
				}
				else
				{
					return new KeyValuePair<ToTypeT, bool>(default!, false); // unable to cast null to ToTypeT
				}
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			if (converter.TryConvert(fromValue.GetValueOrDefault(), out result, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT, bool>(result!, true);

			}
			else
			{
				return new KeyValuePair<ToTypeT, bool>(default!, false);  // conversion failed
			}
		}
		private ToTypeT? AnyToNullable<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider) where ToTypeT : struct
		{
			if (ReferenceEquals(fromValue, null))
			{
				return default;
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result;
		}
		private KeyValuePair<ToTypeT?, bool> TryAnyToNullable<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
			where ToTypeT : struct
		{
			if (ReferenceEquals(fromValue, null))
			{
				return new KeyValuePair<ToTypeT?, bool>(default!, true);
			}

			var converter = this.GetConverter<FromTypeT, ToTypeT>();
			if (converter.TryConvert(fromValue, out var result, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT?, bool>(result, true);
			}
			else
			{
				return new KeyValuePair<ToTypeT?, bool>(default!, false);  // conversion failed
			}
		}
		private ToTypeT DownCast<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (fromValue is ToTypeT toValue)
			{
				return toValue;
			}
			toValue = default!;

			if (ReferenceEquals(fromValue, null))
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (ReferenceEquals(toValue, null))
				{
					return default!;
				}
				else
				{
					throw new InvalidCastException();
				}
			}
			var fromValueType = fromValue.GetType();
			if (fromValueType == typeof(FromTypeT))
			{
				ThrowNoConversionBetweenTypes<FromTypeT, ToTypeT>(fromValue, format, formatProvider);
			}

			var converter = this.GetConverter(fromValueType, typeof(ToTypeT));
			converter.Convert(fromValue, out var result, format, formatProvider);
			return (ToTypeT)result!;
		}
		private KeyValuePair<ToTypeT, bool> TryDownCast<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (fromValue is ToTypeT toValue)
			{
				return new KeyValuePair<ToTypeT, bool>(toValue, true);
			}
			toValue = default!;

			if (ReferenceEquals(fromValue, null))
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (ReferenceEquals(toValue, null))
				{
					return new KeyValuePair<ToTypeT, bool>(default!, true);
				}
				else
				{
					return new KeyValuePair<ToTypeT, bool>(default!, false); // can't convert ValueType to null
				}
			}
			var fromValueType = fromValue.GetType();
			if (fromValueType == typeof(FromTypeT))
			{
				return new KeyValuePair<ToTypeT, bool>(default!, false); // no conversion is found
			}
			var converter = this.GetConverter(fromValueType, typeof(ToTypeT));
			if (converter.TryConvert(fromValue, out var result, format, formatProvider))
			{
				return new KeyValuePair<ToTypeT, bool>((ToTypeT)result!, true);
			}
			return new KeyValuePair<ToTypeT, bool>(default!, false);  // conversion failed

		}
		private static ToTypeT UpCast<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			return (ToTypeT)(object)fromValue!;
		}

		private ToTypeT ConvertEnumToNumber<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var checkedConversion = format != UncheckedConversionFormat;
			var enumConversionInfo = this.GetEnumConversionInfo<FromTypeT>();
			if (typeof(ToTypeT) == typeof(float))
			{
				return (ToTypeT)(object)enumConversionInfo.ToSingle(fromValue!);
			}
			if (typeof(ToTypeT) == typeof(double))
			{
				return (ToTypeT)(object)enumConversionInfo.ToDouble(fromValue!);
			}
			if (typeof(ToTypeT) == typeof(byte))
			{
				return (ToTypeT)(object)enumConversionInfo.ToByte(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(sbyte))
			{
				return (ToTypeT)(object)enumConversionInfo.ToSByte(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(short))
			{
				return (ToTypeT)(object)enumConversionInfo.ToInt16(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(ushort))
			{
				return (ToTypeT)(object)enumConversionInfo.ToUInt16(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(int))
			{
				return (ToTypeT)(object)enumConversionInfo.ToInt32(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(uint))
			{
				return (ToTypeT)(object)enumConversionInfo.ToUInt32(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(long))
			{
				return (ToTypeT)(object)enumConversionInfo.ToInt64(fromValue!, checkedConversion);
			}
			if (typeof(ToTypeT) == typeof(ulong))
			{
				return (ToTypeT)(object)enumConversionInfo.ToUInt64(fromValue!, checkedConversion);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToTypeT)}' while build-in number types are expected.");
		}
		private ToTypeT ConvertNumberToEnum<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var checkedConversion = format != UncheckedConversionFormat;
			var enumConversionInfo = this.GetEnumConversionInfo<ToTypeT>();
			if (typeof(FromTypeT) == typeof(float))
			{
				return enumConversionInfo.FromSingle((float)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(double))
			{
				return enumConversionInfo.FromDouble((double)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(byte))
			{
				return enumConversionInfo.FromByte((byte)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(sbyte))
			{
				return enumConversionInfo.FromSByte((sbyte)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(short))
			{
				return enumConversionInfo.FromInt16((short)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(ushort))
			{
				return enumConversionInfo.FromUInt16((ushort)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(int))
			{
				return enumConversionInfo.FromInt32((int)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(uint))
			{
				return enumConversionInfo.FromUInt32((uint)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(long))
			{
				return enumConversionInfo.FromInt64((long)(object)fromValue!, checkedConversion);
			}
			if (typeof(FromTypeT) == typeof(ulong))
			{
				return enumConversionInfo.FromUInt64((ulong)(object)fromValue!, checkedConversion);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToTypeT)}' while build-in number types are expected.");
		}
		private ToTypeT ConvertEnumToEnum<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var fromEnumConversionInfo = this.GetEnumConversionInfo<FromTypeT>();
			var toEnumConversionInfo = this.GetEnumConversionInfo<ToTypeT>();
			var checkedConversion = format != UncheckedConversionFormat;

			if (fromEnumConversionInfo.IsSigned && toEnumConversionInfo.IsSigned)
			{
				return toEnumConversionInfo.FromInt64(fromEnumConversionInfo.ToInt64(fromValue, checkedConversion), checkedConversion);
			}
			else
			{
				return toEnumConversionInfo.FromUInt64(fromEnumConversionInfo.ToUInt64(fromValue, checkedConversion), checkedConversion);
			}
		}
		private ToTypeT ConvertEnumToString<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<FromTypeT>();
			return (ToTypeT)(object)enumConversionInfo.ToName(fromValue);
		}
		private ToTypeT ConvertStringToEnum<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<ToTypeT>();
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			return enumConversionInfo.Parse((string)(object)fromValue!, ignoreCase);
		}
		private KeyValuePair<ToTypeT, bool> TryConvertStringToEnum<FromTypeT, ToTypeT>(FromTypeT fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<ToTypeT>();
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			var success = enumConversionInfo.TryParse((string)(object)fromValue!, out var result, ignoreCase);
			return new KeyValuePair<ToTypeT, bool>(result, success);
		}

		private static ToTypeT ThrowNoConversionBetweenTypes<FromTypeT, ToTypeT>(FromTypeT _, string? __, IFormatProvider? ___)
		{
			throw new FormatException($"Unable to convert value of type '{typeof(FromTypeT).FullName}' to '{typeof(ToTypeT).FullName}' because there is no conversion method found.");
		}
		//

		/// <summary>
		/// Print information about all known conversions between types.
		/// </summary>
		/// <returns>List of conversions separated with newline.</returns>
		public string DebugPrintConversions()
		{
			var output = new System.Text.StringBuilder();
			foreach (var converterByType in this.converters)
			{
				if (converterByType == null)
				{
					continue;
				}
				foreach (var converter in converterByType)
				{
					if (converter == null)
					{
						continue;
					}

					output.AppendLine(converter.ToString());
				}
			}
			return output.ToString();
		}

		/// <summary>
		/// Prepare conversion between <typeparamref name="FromTypeT"/> and <typeparamref name="ToTypeT"/> for AOT runtime and expose all internal generic method to static analyzer.
		/// </summary>
		public static void PrepareTypesForAotRuntime<FromTypeT, ToTypeT>()
		{
			// this block of code is never executed but visible to static analyzer
			// ReSharper disable All
			if (typeof(TypeConversionProvider).FullName == string.Empty)
			{
#nullable disable
				var instance = new TypeConversionProvider();
				var x = ConversionLookupIndex.FromType<FromTypeT>.ToType<ToTypeT>.ToIndex;
				var y = new Converter<FromTypeT, ToTypeT>(default, default, default);
				instance.CreateConversionDescriptor<FromTypeT, ToTypeT>();
				instance.RegisterConversion<FromTypeT, ToTypeT>(default, default);
				instance.PrepareConvertFunc<FromTypeT, ToTypeT>(default);
				instance.NullableToNullableAot<FromTypeT, ToTypeT>(default, default, default);
				instance.NullableToAnyAot<FromTypeT, ToTypeT>(default, default, default);
				instance.AnyToNullableAot<FromTypeT, ToTypeT>(default, default, default);
				instance.DownCast<FromTypeT, ToTypeT>(default, default, default);
				UpCast<FromTypeT, ToTypeT>(default, default, default);
#if !NETSTANDARD1_6
				if (typeof(FromTypeT).IsEnum)
#endif
				{
					instance.GetEnumConversionInfo<FromTypeT>();
					instance.ConvertEnumToNumber<FromTypeT, ToTypeT>(default, default, default);
					instance.ConvertEnumToString<FromTypeT, ToTypeT>(default, default, default);
					instance.ConvertEnumToEnum<FromTypeT, ToTypeT>(default, default, default);
				}
#if !NETSTANDARD1_6
				if (typeof(ToTypeT).IsEnum)
#endif
				{
					instance.GetEnumConversionInfo<ToTypeT>();
					instance.ConvertNumberToEnum<FromTypeT, ToTypeT>(default, default, default);
					instance.ConvertStringToEnum<FromTypeT, ToTypeT>(default, default, default);
					instance.TryConvertStringToEnum<FromTypeT, ToTypeT>(default, default, default);
				}
				ThrowNoConversionBetweenTypes<FromTypeT, ToTypeT>(default, default, default);
			}
#nullable restore
			// ReSharper enable All
		}

		private static MethodInfo InstantiateGenericMethod<FromTypeT, ToTypeT>(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> methodDelegate, params Type[] typeArguments)
		{
			if (methodDelegate == null) throw new ArgumentNullException(nameof(methodDelegate));
			if (typeArguments == null) throw new ArgumentNullException(nameof(typeArguments));

			var method = methodDelegate.GetMethodInfo();
			var methodDefinition = method.GetGenericMethodDefinition();
			if (methodDefinition == null) throw new InvalidOperationException($"Method '{method}' is not generic and can't be used for generic construction.");
			return methodDefinition.MakeGenericMethod(typeArguments);
		}
		private static MethodInfo InstantiateGenericMethod<FromTypeT, ToTypeT>(Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>> methodDelegate, params Type[] typeArguments)
		{
			if (methodDelegate == null) throw new ArgumentNullException(nameof(methodDelegate));
			if (typeArguments == null) throw new ArgumentNullException(nameof(typeArguments));

			var method = methodDelegate.GetMethodInfo();
			var methodDefinition = method.GetGenericMethodDefinition();
			if (methodDefinition == null) throw new InvalidOperationException($"Method '{method}' is not generic and can't be used for generic construction.");
			return methodDefinition.MakeGenericMethod(typeArguments);
		}

		private static bool IsNumber(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			return type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(byte) ||
				type == typeof(sbyte) ||
				type == typeof(short) ||
				type == typeof(ushort) ||
				type == typeof(int) ||
				type == typeof(uint) ||
				type == typeof(long) ||
				type == typeof(ulong);
		}
	}
}
