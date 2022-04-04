using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private static readonly int ConverterArrayIncrementCount = 20;

		public static readonly string IgnoreCaseFormat = "ignoreCase";

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
		private class TypeConverterAdapter<FromType, ToType>
		{
			private readonly System.ComponentModel.TypeConverter typeConverter;

			public TypeConverterAdapter(System.ComponentModel.TypeConverter typeConverter)
			{
				if (typeConverter == null) throw new ArgumentNullException(nameof(typeConverter));

				this.typeConverter = typeConverter;
			}

			public ToType ConvertTo(FromType fromValue, string? format, IFormatProvider? formatProvider)
			{
				return (ToType)this.typeConverter.ConvertTo(null, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, fromValue, typeof(ToType))!;
			}
			public ToType ConvertFrom(FromType fromValue, string? format, IFormatProvider? formatProvider)
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				return (ToType)this.typeConverter.ConvertFrom(null!, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, fromValue);
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
		/// <param name="registrations">Registrations of custom providers. Alternative way of registering custom conversion is <see cref="TypeConversionProviderConfiguration"/>.</param>
#pragma warning restore 1572
		public TypeConversionProvider(
#if NET45
			TypeConversionProviderConfiguration? configuration = null,
#else
			Microsoft.Extensions.Options.IOptions<TypeConversionProviderConfiguration>? configurationOptions = null,

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
				foreach(var registration in registrations)
				{
					registration.Register(this);
				}
			}
		}

		/// <inheritdoc />
		public IConverter<FromType, ToType> GetConverter<FromType, ToType>()
		{
			var fromTypeIndex = ConversionLookupIndex.FromType<FromType>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromType>.ToType<ToType>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			if (this.GetType().Name == string.Empty)
			{
				// AOT static compilation hack
				PrepareTypesForAotRuntime<FromType, ToType>();
			}

			if (toConverters[toTypeIndex] is IConverter<FromType, ToType> converter)
			{
				return converter;
			}
			else
			{
				var conversionDescriptor = this.CreateConversionDescriptor<FromType, ToType>();
				converter = new Converter<FromType, ToType>(conversionDescriptor, this.converterOptions);
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
			getConverterFunc = ReflectionExtensions.CreateDelegate<Func<IConverter>>(this, getConverterMethod, throwOnBindFailure: true) ?? throw new InvalidOperationException();

			this.getConverterByTypes[typePairIndex] = getConverterFunc;

			return getConverterFunc();
		}

		/// <inheritdoc />
		public void RegisterConversion<FromType, ToType>(Func<FromType, string, IFormatProvider, ToType> conversionFunc, ConversionQuality quality)
		{
			var conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFunc, quality) };
			var fromTypeIndex = ConversionLookupIndex.FromType<FromType>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromType>.ToType<ToType>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			var conversionDescriptor = new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(conversionMethods), null, this.defaultFormatProvider, conversionFunc, default(Delegate));
			var converter = new Converter<FromType, ToType>(conversionDescriptor, this.converterOptions);
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

		private ConversionDescriptor CreateConversionDescriptor<FromType, ToType>()
		{
			var fromType = typeof(FromType);
			var toType = typeof(ToType);

			var conversionMethods = default(ConversionMethodInfo[]); // from best to worst
			var conversionFn = default(Func<FromType, string?, IFormatProvider?, ToType>);
			var safeConversionFn = default(Func<FromType, string?, IFormatProvider?, KeyValuePair<ToType, bool>>);
			var defaultFormat = default(string);
			var defaultFormatProvider = this.defaultFormatProvider;

			// fallback conversion is used when no conversion method is found on types
			var fallbackConversionFn = default(Func<FromType, string?, IFormatProvider?, ToType>?);
			var fallbackConversionMethodInfo = default(ConversionMethodInfo);

			switch (this.DetectNativeConversion(fromType, toType))
			{
				case KnownNativeConversion.UpCasting:
					conversionFn = UpCast<FromType, ToType>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.DownCasting:
					fallbackConversionFn = this.DownCast<FromType, ToType>;
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.NullableToNullable:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = NullableToNullableAot<FromType, ToType>;
					}
					else
					{
						var nullableToNullableMethod = new Func<int?, string, IFormatProvider, int?>(NullableToNullable<int, int>).GetMethodInfo()!.GetGenericMethodDefinition()
							.MakeGenericMethod(Nullable.GetUnderlyingType(fromType)!, Nullable.GetUnderlyingType(toType)!);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromType, string?, IFormatProvider?, ToType>>(this, nullableToNullableMethod) ?? throw new InvalidOperationException();
					}

					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.NullableToAny:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = NullableToAnyAot<FromType, ToType>;
					}
					else
					{
						var nullableToNullableMethod = new Func<int?, string, IFormatProvider, int>(NullableToAny<int, int>).GetMethodInfo()!.GetGenericMethodDefinition()
							.MakeGenericMethod(Nullable.GetUnderlyingType(fromType)!, toType);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromType, string?, IFormatProvider?, ToType>>(this, nullableToNullableMethod) ?? throw new InvalidOperationException();
					}
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.AnyToNullable:
					if ((this.converterOptions & ConversionOptions.OptimizeWithGenerics) == 0)
					{
						fallbackConversionFn = AnyToNullableAot<FromType, ToType>;
					}
					else
					{
						var nullableToNullableMethod = new Func<int, string, IFormatProvider, int?>(AnyToNullable<int, int>).GetMethodInfo()!.GetGenericMethodDefinition()
							.MakeGenericMethod(fromType, Nullable.GetUnderlyingType(toType)!);
						fallbackConversionFn = ReflectionExtensions.CreateDelegate<Func<FromType, string?, IFormatProvider?, ToType>>(this, nullableToNullableMethod) ?? throw new InvalidOperationException();
					}
					fallbackConversionMethodInfo = ConversionMethodInfo.FromNativeConversion(fallbackConversionFn);
					goto default;
				case KnownNativeConversion.EnumToNumber:
					conversionFn = ConvertEnumToNumber<FromType, ToType>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.EnumToEnum:
					conversionFn = ConvertEnumToEnum<FromType, ToType>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.NumberToEnum:
					conversionFn = ConvertNumberToEnum<FromType, ToType>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.EnumToString:
					conversionFn = ConvertEnumToString<FromType, ToType>;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn) };
					break;
				case KnownNativeConversion.StringToEnum:
					conversionFn = ConvertStringToEnum<FromType, ToType>;
					safeConversionFn = ConvertStringToEnumSafe<FromType, ToType>;
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
				if (toTypeConverter?.CanConvertFrom(fromType) ?? false)
				{
					var adapter = new TypeConverterAdapter<FromType, ToType>(toTypeConverter!);
					conversionFn = adapter.ConvertFrom;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.TypeConverter) };
				}

				var fromTypeConverter = this.metadataProvider.GetTypeConverter(fromType);
				if (conversionMethods.Length == 0 && (fromTypeConverter?.CanConvertTo(toType) ?? false))
				{
					var adapter = new TypeConverterAdapter<FromType, ToType>(fromTypeConverter!);
					conversionFn = adapter.ConvertTo;
					conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.TypeConverter) };
				}
			}
#endif
			if (conversionMethods.Length == 0 && fallbackConversionMethodInfo != null)
			{
				conversionMethods = new[] { fallbackConversionMethodInfo };
				conversionFn = fallbackConversionFn;
			}

			if (conversionMethods.Length == 0)
			{
				conversionFn = ThrowNoConversionBetweenTypes<FromType, ToType>;
				conversionMethods = new[] { ConversionMethodInfo.FromNativeConversion(conversionFn, ConversionQuality.None) };
				safeConversionFn = (_, _, _) => new KeyValuePair<ToType, bool>(default!, false);
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
					conversionFn = this.PrepareConvertFunc<FromType, ToType>(conversionMethods);
				}
				else
				{
					conversionFn = this.PrepareConvertExpression<FromType, ToType>(conversionMethods);
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

		private Func<FromType, string?, IFormatProvider?, ToType> PrepareConvertExpression<FromType, ToType>(params ConversionMethodInfo[] methods)
		{
			if (methods == null) throw new ArgumentNullException(nameof(methods));
			if (methods.Length == 0) throw new ArgumentOutOfRangeException(nameof(methods));

			var fromType = typeof(FromType);
			var toType = typeof(ToType);
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

			return Expression.Lambda<Func<FromType, string?, IFormatProvider?, ToType>>(
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
					throw new InvalidOperationException(
						$"Invalid conversion method: {conversionMethodInfo.Method}. This should be instance of '{typeof(MethodInfo)}' or '{typeof(ConstructorInfo)}'.");
				}

				if (convertCallExpression.Type != typeof(ToType))
				{
					convertCallExpression = Expression.ConvertChecked(convertCallExpression, typeof(ToType));
				}
				return convertCallExpression;
			}
		}
		private Func<FromType, string?, IFormatProvider?, ToType> PrepareConvertFunc<FromType, ToType>(params ConversionMethodInfo[] methods)
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
								return (ToType)methodInfo.Invoke(null, arguments)!;
							}
							else if (ReferenceEquals(fromValue, null))
							{
								return default;
							}
							else
							{
								return (ToType)methodInfo.Invoke(fromValue, arguments)!;
							}
						}
						else if (method.Method is ConstructorInfo constructorInfo)
						{
							return (ToType)constructorInfo.Invoke(arguments);
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
		private ToType NullableToNullableAot<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
				return default(ToType)!;

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromType)) ?? typeof(FromType);
			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToType)) ?? typeof(ToType);

			this.GetConverter(fromUnderlyingType, toUnderlyingType).Convert(fromValue, out var result, format, formatProvider);
			return (ToType)result!;
		}
		private ToType NullableToAnyAot<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var result = default(ToType);
			if (ReferenceEquals(fromValue, null))
			{
				if (ReferenceEquals(result, null))
					return default(ToType)!;
				throw new InvalidCastException($"Unable to convert <null> to '{typeof(ToType)}'.");
			}

			var fromUnderlyingType = Nullable.GetUnderlyingType(typeof(FromType)) ?? typeof(FromType);

			this.GetConverter(fromUnderlyingType, typeof(ToType)).Convert(fromValue, out var resultObj, format, formatProvider);
			return (ToType)resultObj!;
		}
		private ToType AnyToNullableAot<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (ReferenceEquals(fromValue, null))
				return default(ToType)!;

			var toUnderlyingType = Nullable.GetUnderlyingType(typeof(ToType)) ?? typeof(ToType);

			this.GetConverter(typeof(FromType), toUnderlyingType).Convert(fromValue, out var result, format, formatProvider);
			return (ToType)result!;
		}
		private ToType? NullableToNullable<FromType, ToType>(FromType? fromValue, string? format, IFormatProvider? formatProvider) where ToType : struct where FromType : struct
		{
			if (fromValue.HasValue == false)
				return default(ToType?);

			this.GetConverter<FromType, ToType>().Convert(fromValue!.Value, out var result, format, formatProvider);
			return result;
		}
		private ToType NullableToAny<FromType, ToType>(FromType? fromValue, string? format, IFormatProvider? formatProvider) where FromType : struct
		{
			var result = default(ToType);
			if (fromValue.HasValue == false)
			{
				if (ReferenceEquals(result, null))
					return default(ToType)!;
				throw new InvalidCastException($"Unable to convert <null> to '{typeof(ToType)}'.");
			}

			this.GetConverter<FromType, ToType>().Convert(fromValue.GetValueOrDefault(), out result, format, formatProvider);
			return result!;
		}
		private ToType? AnyToNullable<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider) where ToType : struct
		{
			if (ReferenceEquals(fromValue, null))
				return default(ToType?);

			this.GetConverter<FromType, ToType>().Convert(fromValue, out var result, format, formatProvider);
			return result;
		}
		private ToType DownCast<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (fromValue is ToType toValue)
			{
				return toValue;
			}
			toValue = default(ToType)!;

			if (ReferenceEquals(fromValue, null))
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (ReferenceEquals(toValue, null))
				{
					return default(ToType)!;
				}
				else
				{
					throw new InvalidCastException();
				}
			}
			var fromValueType = fromValue.GetType();
			if (fromValueType == typeof(FromType))
			{
				ThrowNoConversionBetweenTypes<FromType, ToType>(fromValue, format, formatProvider);
			}
			this.GetConverter(fromValueType, typeof(ToType)).Convert(fromValue, out var result, format, formatProvider);
			return (ToType)result!;
		}
		private static ToType UpCast<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			return (ToType)(object)fromValue!;
		}

		private ToType ConvertEnumToNumber<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<FromType>();
			if (typeof(ToType) == typeof(float))
			{
				return (ToType)(object)enumConversionInfo.ToSingle(fromValue!);
			}
			if (typeof(ToType) == typeof(double))
			{
				return (ToType)(object)enumConversionInfo.ToDouble(fromValue!);
			}
			if (typeof(ToType) == typeof(byte))
			{
				return (ToType)(object)enumConversionInfo.ToByte(fromValue!);
			}
			if (typeof(ToType) == typeof(sbyte))
			{
				return (ToType)(object)enumConversionInfo.ToSByte(fromValue!);
			}
			if (typeof(ToType) == typeof(short))
			{
				return (ToType)(object)enumConversionInfo.ToInt16(fromValue!);
			}
			if (typeof(ToType) == typeof(ushort))
			{
				return (ToType)(object)enumConversionInfo.ToUInt16(fromValue!);
			}
			if (typeof(ToType) == typeof(int))
			{
				return (ToType)(object)enumConversionInfo.ToInt32(fromValue!);
			}
			if (typeof(ToType) == typeof(uint))
			{
				return (ToType)(object)enumConversionInfo.ToUInt32(fromValue!);
			}
			if (typeof(ToType) == typeof(long))
			{
				return (ToType)(object)enumConversionInfo.ToInt64(fromValue!);
			}
			if (typeof(ToType) == typeof(ulong))
			{
				return (ToType)(object)enumConversionInfo.ToUInt64(fromValue!);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToType)}' while build-in number types are expected.");
		}
		private ToType ConvertNumberToEnum<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<ToType>();
			if (typeof(FromType) == typeof(float))
			{
				return enumConversionInfo.FromSingle((float)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(double))
			{
				return enumConversionInfo.FromDouble((double)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(byte))
			{
				return enumConversionInfo.FromByte((byte)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(sbyte))
			{
				return enumConversionInfo.FromSByte((sbyte)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(short))
			{
				return enumConversionInfo.FromInt16((short)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(ushort))
			{
				return enumConversionInfo.FromUInt16((ushort)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(int))
			{
				return enumConversionInfo.FromInt32((int)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(uint))
			{
				return enumConversionInfo.FromUInt32((uint)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(long))
			{
				return enumConversionInfo.FromInt64((long)(object)fromValue!);
			}
			if (typeof(FromType) == typeof(ulong))
			{
				return enumConversionInfo.FromUInt64((ulong)(object)fromValue!);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToType)}' while build-in number types are expected.");
		}
		private ToType ConvertEnumToEnum<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var fromEnumConversionInfo = this.GetEnumConversionInfo<FromType>();
			var toEnumConversionInfo = this.GetEnumConversionInfo<ToType>();

			if (fromEnumConversionInfo.IsSigned && toEnumConversionInfo.IsSigned)
			{
				return toEnumConversionInfo.FromInt64(fromEnumConversionInfo.ToInt64(fromValue));
			}
			else
			{
				return toEnumConversionInfo.FromUInt64(fromEnumConversionInfo.ToUInt64(fromValue));
			}
		}
		private ToType ConvertEnumToString<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<FromType>();
			return (ToType)(object)enumConversionInfo.ToName(fromValue);
		}
		private ToType ConvertStringToEnum<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<ToType>();
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			return enumConversionInfo.Parse((string)(object)fromValue!, ignoreCase);
		}
		private KeyValuePair<ToType, bool> ConvertStringToEnumSafe<FromType, ToType>(FromType fromValue, string? format, IFormatProvider? formatProvider)
		{
			var enumConversionInfo = this.GetEnumConversionInfo<ToType>();
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			var success = enumConversionInfo.TryParse((string)(object)fromValue!, out var result, ignoreCase);
			return new KeyValuePair<ToType, bool>(result, success);
		}
		private static ToType ThrowNoConversionBetweenTypes<FromType, ToType>(FromType _, string? __, IFormatProvider? ___)
		{
			throw new InvalidOperationException(
				$"Unable to convert value of type '{typeof(FromType).FullName}' to '{typeof(ToType).FullName}' because there is no conversion method found.");
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
		/// Prepare conversion between <typeparamref name="FromType"/> and <typeparamref name="ToType"/> for AOT runtime and expose all internal generic method to static analyzer.
		/// </summary>
		public static void PrepareTypesForAotRuntime<FromType, ToType>()
		{
			// this block of code is never executed but visible to static analyzer
			// ReSharper disable All
			if (typeof(TypeConversionProvider).FullName == string.Empty)
			{
#nullable disable
				var instance = new TypeConversionProvider();
				var x = ConversionLookupIndex.FromType<FromType>.ToType<ToType>.ToIndex;
				var y = new Converter<FromType, ToType>(default, default);
				instance.CreateConversionDescriptor<FromType, ToType>();
				instance.RegisterConversion<FromType, ToType>(default, default);
				instance.PrepareConvertFunc<FromType, ToType>(default);
				instance.NullableToNullableAot<FromType, ToType>(default, default, default);
				instance.NullableToAnyAot<FromType, ToType>(default, default, default);
				instance.AnyToNullableAot<FromType, ToType>(default, default, default);
				instance.DownCast<FromType, ToType>(default, default, default);
				UpCast<FromType, ToType>(default, default, default);
#if !NETSTANDARD1_6
				if (typeof(FromType).IsEnum)
#endif
				{
					instance.GetEnumConversionInfo<FromType>();
					instance.ConvertEnumToNumber<FromType, ToType>(default, default, default);
					instance.ConvertEnumToString<FromType, ToType>(default, default, default);
					instance.ConvertEnumToEnum<FromType, ToType>(default, default, default);
				}
#if !NETSTANDARD1_6
				if (typeof(ToType).IsEnum)
#endif
				{
					instance.GetEnumConversionInfo<ToType>();
					instance.ConvertNumberToEnum<FromType, ToType>(default, default, default);
					instance.ConvertStringToEnum<FromType, ToType>(default, default, default);
					instance.ConvertStringToEnumSafe<FromType, ToType>(default, default, default);
				}
				ThrowNoConversionBetweenTypes<FromType, ToType>(default, default, default);
			}
#nullable restore
			// ReSharper enable All
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
