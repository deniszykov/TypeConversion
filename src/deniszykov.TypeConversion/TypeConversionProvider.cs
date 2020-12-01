using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public partial class TypeConversionProvider : ITypeConversionProvider
	{
		private static readonly int ConverterArrayIncrementCount = 20;

		public static readonly string IgnoreCaseFormat = "ignoreCase";

		private static class ConversionLookupIndex
		{
			private static int LastFromIndex = -1;

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
			// ReSharper restore StaticMemberInGenericType, UnusedTypeParameter

		}
		private enum ConversionClass
		{
			Unknown = 0,
			EnumToNumber,
			EnumToEnum,
			NumberToEnum,
			EnumToString,
			StringToEnum,
			UpCasting,
			DownCasting,
		}

		private IConverter[][] converters;
		private readonly MethodInfo getConverterDefinition;
		private readonly Dictionary<long, Func<IConverter>> getConverterByTypes;
		private readonly IConversionMetadataProvider metadataProvider;
		private readonly bool isAotRuntime;

		public TypeConversionProvider(
#if NETFRAMEWORK
			[CanBeNull] TypeConversionProviderConfiguration configuration = null,
#else
			[CanBeNull] Microsoft.Extensions.Options.IOptions<TypeConversionProviderConfiguration> configurationOptions = null,

#endif
			[CanBeNull] IConversionMetadataProvider metadataProvider = null
		)
		{
#if !NETFRAMEWORK
			var configuration = configurationOptions?.Value;
#endif
			this.converters = new IConverter[ConverterArrayIncrementCount][];
			this.getConverterByTypes = new Dictionary<long, Func<IConverter>>();
			this.getConverterDefinition = new Func<IConverter>(this.GetConverter<object, object>).GetMethodInfo().GetGenericMethodDefinition();
			this.metadataProvider = metadataProvider ?? new ConversionMetadataProvider();
			this.isAotRuntime = configuration?.IsAotRuntime ?? false;

			this.InitializeNativeConversions();
			this.InitializeCustomConversion();
		}

		public IConverter<FromType, ToType> GetConverter<FromType, ToType>()
		{
			var fromTypeIndex = ConversionLookupIndex.FromType<FromType>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromType>.ToType<ToType>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			if (toConverters[toTypeIndex] is IConverter<FromType, ToType> converter)
			{
				return converter;
			}
			else
			{
				var conversionInfo = this.CreateConversionInfo<FromType, ToType>();
				converter = new Converter<FromType, ToType>(conversionInfo);
				toConverters[toTypeIndex] = converter;
				return converter;
			}
		}
		public IConverter GetConverter(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var fromHash = fromType.GetHashCode(); // it's not hashcode, it's an unique sync-lock of type-object
			var toHash = toType.GetHashCode();
			var typePairIndex = unchecked(((long)fromHash << 32) | (uint)toHash);
			var getConverterFunc = default(Func<IConverter>);

			lock (this.getConverterByTypes)
			{
				if (this.getConverterByTypes.TryGetValue(typePairIndex, out getConverterFunc))
				{
					return getConverterFunc();
				}
			}

			var getConverterMethod = this.getConverterDefinition.MakeGenericMethod(fromType, toType);
			if (this.isAotRuntime)
			{
				var emptyArguments = new object[0];
				getConverterFunc = () => (IConverter)getConverterMethod.Invoke(this, emptyArguments);
			}
			else
			{
				getConverterFunc = ReflectionExtensions.CreateDelegate<Func<IConverter>>(this, getConverterMethod);
			}

			lock (this.getConverterByTypes)
			{
				this.getConverterByTypes[typePairIndex] = getConverterFunc;
			}

			return getConverterFunc();
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
				toConverters = Interlocked.CompareExchange(ref this.converters[fromTypeIndex], new IConverter[ConverterArrayIncrementCount], null);
			}

			while (toTypeIndex >= toConverters.Length)
			{
				var originalToConverters = toConverters;
				Array.Resize(ref toConverters, toConverters.Length + ConverterArrayIncrementCount);
				toConverters = Interlocked.CompareExchange(ref this.converters[fromTypeIndex], toConverters, originalToConverters);
			}

			return toConverters;
		}

		private ConversionInfo CreateConversionInfo<FromType, ToType>()
		{
			var fromType = typeof(FromType);
			var toType = typeof(ToType);

			var conversionFn = default(Func<FromType, string, IFormatProvider, ToType>);
			var safeConversionFn = default(Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>);
			var defaultFormat = default(string);

			var conversionMethodInfo = default(ConversionMethodInfo);
			switch (this.GetConversionClass(fromType, toType))
			{
				case ConversionClass.UpCasting:
					conversionFn = UpCast<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.DownCasting:
					conversionFn = DownCast<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.EnumToNumber:
					conversionFn = ConvertEnumToNumber<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.EnumToEnum:
					conversionFn = ConvertEnumToEnum<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.NumberToEnum:
					conversionFn = ConvertNumberToEnum<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.EnumToString:
					conversionFn = ConvertEnumToString<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.StringToEnum:
					conversionFn = ConvertStringToEnum<FromType, ToType>;
					safeConversionFn = ConvertStringToEnumSafe<FromType, ToType>;
					conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.Native);
					break;
				case ConversionClass.Unknown:
				default:
					conversionMethodInfo = this.FindConversionBetweenTypes(fromType, toType);
					break;
			}

			if (conversionMethodInfo == null)
			{
				conversionFn = ThrowNoConversionBetweenTypes<FromType, ToType>;
				conversionMethodInfo = new ConversionMethodInfo(conversionFn.GetMethodInfo(), 0, conversionQualityOverride: ConversionQuality.None);
				safeConversionFn = (fromValue, format, formatProvider) => new KeyValuePair<ToType, bool>(default, false);
			}
			else if (conversionFn == null)
			{
				defaultFormat = this.metadataProvider.GetDefaultFormat(conversionMethodInfo);
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

				if (this.isAotRuntime)
				{
					conversionFn = this.PrepareConvertFunc<FromType, ToType>(conversionMethodInfo);
				}
				else
				{
					conversionFn = this.PrepareConvertExpression<FromType, ToType>(conversionMethodInfo);
				}
			}
			return new ConversionInfo(conversionMethodInfo, defaultFormat, conversionFn, safeConversionFn);
		}

		private ConversionClass GetConversionClass([NotNull] Type fromType, [NotNull] Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			if (this.metadataProvider.IsAssignableFrom(toType, fromType))
			{
				return ConversionClass.UpCasting;
			}
			else if (this.metadataProvider.IsAssignableFrom(fromType, toType))
			{
				return ConversionClass.DownCasting;
			}

			var fromTypeInfo = fromType.GetTypeInfo();
			var toTypeInfo = toType.GetTypeInfo();

			if (fromTypeInfo.IsEnum && toTypeInfo.IsEnum)
			{
				return ConversionClass.EnumToEnum;
			}
			else if (fromTypeInfo.IsEnum && IsNumber(toType))
			{
				return ConversionClass.EnumToNumber;
			}
			else if (toTypeInfo.IsEnum && IsNumber(fromType))
			{
				return ConversionClass.NumberToEnum;
			}
			else if (fromTypeInfo.IsEnum && toType == typeof(string))
			{
				return ConversionClass.EnumToString;
			}
			else if (toTypeInfo.IsEnum && fromType == typeof(string))
			{
				return ConversionClass.StringToEnum;
			}
			else
			{
				return ConversionClass.Unknown;
			}
		}

		[CanBeNull]
		private ConversionMethodInfo FindConversionBetweenTypes([NotNull] Type fromType, [NotNull] Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var conversionMethodInfo = default(ConversionMethodInfo);
			foreach (var convertFromMethod in this.metadataProvider.GetConvertFromMethods(toType))
			{
				if (this.metadataProvider.IsAssignableFrom(convertFromMethod.FromType, fromType) &&
					this.metadataProvider.IsAssignableFrom(toType, convertFromMethod.ToType))
				{
					conversionMethodInfo = ConversionMethodInfo.ChooseByQuality(conversionMethodInfo, convertFromMethod);
				}
			}
			foreach (var convertFromMethod in this.metadataProvider.GetConvertToMethods(fromType))
			{
				if (this.metadataProvider.IsAssignableFrom(convertFromMethod.FromType, fromType) &&
					this.metadataProvider.IsAssignableFrom(toType, convertFromMethod.ToType))
				{
					conversionMethodInfo = ConversionMethodInfo.ChooseByQuality(conversionMethodInfo, convertFromMethod);
				}
			}
			return conversionMethodInfo;
		}

		private void RegisterConverter<FromType, ToType>([NotNull] Func<FromType, string, IFormatProvider, ToType> conversionFunc, ConversionQuality quality)
		{
			var conversionMethod = conversionFunc.GetMethodInfo();
			var conversionMethodInfo = new ConversionMethodInfo(conversionMethod, 0, conversionQualityOverride: quality);
			var fromTypeIndex = ConversionLookupIndex.FromType<FromType>.FromIndex;
			var toTypeIndex = ConversionLookupIndex.FromType<FromType>.ToType<ToType>.ToIndex;
			var toConverters = this.GetToConverters(fromTypeIndex, toTypeIndex);

			var conversionInfo = new ConversionInfo(conversionMethodInfo, null, conversionFunc, default(Delegate));
			var converter = new Converter<FromType, ToType>(conversionInfo);
			toConverters[toTypeIndex] = converter;
		}

		private void InitializeCustomConversion()
		{
			this.RegisterConverter<string, Uri>((value, format, fp) =>
			{
				var kind = string.IsNullOrEmpty(format) ? UriKind.RelativeOrAbsolute : (UriKind)Enum.Parse(typeof(UriKind), format, ignoreCase: true);
				return new Uri(value, kind);
			}, ConversionQuality.Custom);

			this.RegisterConverter<Uri, string>((value, format, fp) => value.OriginalString, ConversionQuality.Custom);

			this.RegisterConverter<string, DateTime>((str, f, fp) =>
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

		[NotNull]
		private Func<FromType, string, IFormatProvider, ToType> PrepareConvertExpression<FromType, ToType>([NotNull] ConversionMethodInfo conversionMethodInfo)
		{
			var fromType = typeof(FromType);
			var toType = typeof(ToType);
			var methodParameters = conversionMethodInfo.Parameters;
			var fromValueParameter = Expression.Parameter(fromType, "fromValue");
			var formatParameter = Expression.Parameter(typeof(string), "format");
			var formatProviderParameter = Expression.Parameter(typeof(IFormatProvider), "formatProvider");
			var arguments = new Expression[methodParameters.Length];

			for (var i = 0; i < methodParameters.Length; i++)
			{
				if (this.metadataProvider.IsFormatParameter(methodParameters[i]))
				{
					arguments[i] = formatParameter;
				}
				else if (this.metadataProvider.IsFormatProviderParameter(methodParameters[i]))
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

			var convertExpression = default(Expression);
			if (conversionMethodInfo.Method is MethodInfo methodInfo)
			{
				if (methodInfo.IsStatic)
				{
					convertExpression = Expression.Call(methodInfo, arguments);
				}
				else
				{
					var callTarget = (Expression)fromValueParameter;
					if (callTarget.Type != methodInfo.DeclaringType && methodInfo.DeclaringType != null)
					{
						callTarget = Expression.ConvertChecked(callTarget, methodInfo.DeclaringType);
					}

					convertExpression = Expression.Call(callTarget, methodInfo, arguments);
				}
			}
			else if (conversionMethodInfo.Method is ConstructorInfo constructorInfo)
			{
				convertExpression = Expression.New(constructorInfo, arguments);
			}
			else
			{
				throw new InvalidOperationException(
					$"Invalid conversion method: {conversionMethodInfo.Method}. This should be instance of '{typeof(MethodInfo)}' or '{typeof(ConstructorInfo)}'.");
			}

			if (convertExpression.Type != typeof(ToType))
			{
				convertExpression = Expression.ConvertChecked(convertExpression, typeof(ToType));
			}

			return Expression.Lambda<Func<FromType, string, IFormatProvider, ToType>>(
				convertExpression,
				$"Convert_{fromType.Name}_{toType.Name}_via_{conversionMethodInfo.Method.Name}_{string.Join("_", conversionMethodInfo.Parameters.Select(p => p.Name))}",
				new[] {
					fromValueParameter,
					formatParameter,
					formatProviderParameter
				}
			).Compile();
		}
		[NotNull]
		private Func<FromType, string, IFormatProvider, ToType> PrepareConvertFunc<FromType, ToType>([NotNull] ConversionMethodInfo conversionMethodInfo)
		{
			var methodParameters = conversionMethodInfo.Parameters;
			var formatParameterIndex = -1;
			var formatProviderParameterIndex = -1;
			var fromValueParameterIndex = -1;

			for (var i = 0; i < methodParameters.Length; i++)
			{
				if (this.metadataProvider.IsFormatParameter(methodParameters[i]))
				{
					formatParameterIndex = i;
				}
				else if (this.metadataProvider.IsFormatProviderParameter(methodParameters[i]))
				{
					formatProviderParameterIndex = i;
				}
				else
				{
					fromValueParameterIndex = i;
				}
			}

			return (fromValue, format, formatProvider) =>
			{
				var arguments = new object[methodParameters.Length];
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

				if (conversionMethodInfo.Method is MethodInfo methodInfo)
				{
					if (methodInfo.IsStatic)
					{
						return (ToType)methodInfo.Invoke(null, arguments);
					}
					else
					{
						return (ToType)methodInfo.Invoke(fromValue, arguments);
					}
				}
				else if (conversionMethodInfo.Method is ConstructorInfo constructorInfo)
				{
					return (ToType)constructorInfo.Invoke(arguments);
				}
				else
				{
					throw new InvalidOperationException($"Invalid conversion method: {conversionMethodInfo.Method}. This should be instance of '{typeof(MethodInfo)}' or '{typeof(ConstructorInfo)}'.");
				}
			};
		}

		private ToType DownCast<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			if (fromValue is ToType toValue)
			{
				return toValue;
			}
			toValue = default(ToType);

			if (ReferenceEquals(fromValue, null))
			{
				if (ReferenceEquals(toValue, null))
				{
					return default(ToType);
				}
				else
				{
					throw new InvalidCastException();
				}
			}
			var fromValueType = fromValue.GetType();
			this.GetConverter(fromValueType, typeof(ToType)).Convert(fromValue, out var result, format, formatProvider);
			return (ToType)result;
		}
		private ToType UpCast<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			return (ToType)(object)fromValue;
		}
		private ToType ConvertEnumToNumber<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			if (typeof(ToType) == typeof(float))
			{
				return (ToType)(object)EnumHelper<FromType>.ToSingle(fromValue);
			}
			if (typeof(ToType) == typeof(double))
			{
				return (ToType)(object)EnumHelper<FromType>.ToDouble(fromValue);
			}
			if (typeof(ToType) == typeof(byte))
			{
				return (ToType)(object)EnumHelper<FromType>.ToByte(fromValue);
			}
			if (typeof(ToType) == typeof(sbyte))
			{
				return (ToType)(object)EnumHelper<FromType>.ToSByte(fromValue);
			}
			if (typeof(ToType) == typeof(short))
			{
				return (ToType)(object)EnumHelper<FromType>.ToInt16(fromValue);
			}
			if (typeof(ToType) == typeof(ushort))
			{
				return (ToType)(object)EnumHelper<FromType>.ToUInt16(fromValue);
			}
			if (typeof(ToType) == typeof(int))
			{
				return (ToType)(object)EnumHelper<FromType>.ToInt32(fromValue);
			}
			if (typeof(ToType) == typeof(uint))
			{
				return (ToType)(object)EnumHelper<FromType>.ToUInt32(fromValue);
			}
			if (typeof(ToType) == typeof(long))
			{
				return (ToType)(object)EnumHelper<FromType>.ToInt64(fromValue);
			}
			if (typeof(ToType) == typeof(ulong))
			{
				return (ToType)(object)EnumHelper<FromType>.ToUInt64(fromValue);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToType)}' while build-in number types are expected.");
		}
		private ToType ConvertNumberToEnum<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			if (typeof(FromType) == typeof(float))
			{
				return EnumHelper<ToType>.FromSingle((float)(object)fromValue);
			}
			if (typeof(FromType) == typeof(double))
			{
				return EnumHelper<ToType>.FromDouble((double)(object)fromValue);
			}
			if (typeof(FromType) == typeof(byte))
			{
				return EnumHelper<ToType>.FromByte((byte)(object)fromValue);
			}
			if (typeof(FromType) == typeof(sbyte))
			{
				return EnumHelper<ToType>.FromSByte((sbyte)(object)fromValue);
			}
			if (typeof(FromType) == typeof(short))
			{
				return EnumHelper<ToType>.FromInt16((short)(object)fromValue);
			}
			if (typeof(FromType) == typeof(ushort))
			{
				return EnumHelper<ToType>.FromUInt16((ushort)(object)fromValue);
			}
			if (typeof(FromType) == typeof(int))
			{
				return EnumHelper<ToType>.FromInt32((int)(object)fromValue);
			}
			if (typeof(FromType) == typeof(uint))
			{
				return EnumHelper<ToType>.FromUInt32((uint)(object)fromValue);
			}
			if (typeof(FromType) == typeof(long))
			{
				return EnumHelper<ToType>.FromInt64((long)(object)fromValue);
			}
			if (typeof(FromType) == typeof(ulong))
			{
				return EnumHelper<ToType>.FromUInt64((ulong)(object)fromValue);
			}

			throw new InvalidOperationException($"Unknown number type specified '{typeof(ToType)}' while build-in number types are expected.");
		}
		private ToType ConvertEnumToEnum<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			if (EnumHelper<FromType>.IsSigned && EnumHelper<ToType>.IsSigned)
			{
				return EnumHelper<ToType>.FromInt64(EnumHelper<FromType>.ToInt64(fromValue));
			}
			else
			{
				return EnumHelper<ToType>.FromUInt64(EnumHelper<FromType>.ToUInt64(fromValue));
			}
		}
		private ToType ConvertEnumToString<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			return (ToType)(object)EnumHelper<FromType>.ToName(fromValue);
		}
		private ToType ConvertStringToEnum<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			return (ToType)(object)EnumHelper<ToType>.Parse((string)(object)fromValue, ignoreCase);
		}
		private KeyValuePair<ToType, bool> ConvertStringToEnumSafe<FromType, ToType>(FromType fromValue, string format, IFormatProvider formatProvider)
		{
			var ignoreCase = string.Equals(format, IgnoreCaseFormat, StringComparison.OrdinalIgnoreCase);
			var success = EnumHelper<ToType>.TryParse((string)(object)fromValue, out var result, ignoreCase);
			return new KeyValuePair<ToType, bool>(result, success);
		}
		private ToType ThrowNoConversionBetweenTypes<FromType, ToType>(FromType _, string __, IFormatProvider ___)
		{
			throw new InvalidOperationException(
				$"Unable to convert value of type '{typeof(FromType).FullName}' to '{typeof(ToType).FullName}' because there is no conversion method found.");
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
