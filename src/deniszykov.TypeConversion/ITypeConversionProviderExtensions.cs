

using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public static class ITypeConversionProviderExtensions
	{
		[CanBeNull]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue)
		{
			return Convert(typeConversionProvider, fromType, toType, fromValue, default(string), default(IFormatProvider));
		}
		[CanBeNull]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] string format)
		{
			return Convert(typeConversionProvider, fromType, toType, fromValue, format, default(IFormatProvider));
		}
		[CanBeNull]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			return Convert(typeConversionProvider, fromType, toType, fromValue, default(string), formatProvider);
		}
		[CanBeNull]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			typeConversionProvider.GetConverter(fromType, toType).Convert(fromValue, out var result, format, formatProvider);
			return result;
		}

		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result)
		{
			return TryConvert(typeConversionProvider, fromType, toType, fromValue, out result, default(string), default(IFormatProvider));
		}
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] string format)
		{
			return TryConvert(typeConversionProvider, fromType, toType, fromValue, out result, format, default(IFormatProvider));
		}
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] IFormatProvider formatProvider)
		{
			return TryConvert(typeConversionProvider, fromType, toType, fromValue, out result, default(string), formatProvider);
		}
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			return typeConversionProvider.GetConverter(fromType, toType).TryConvert(fromValue, out result, format, formatProvider);
		}

		[CanBeNull]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue)
		{
			return Convert<FromType, ToType>(typeConversionProvider, fromValue, default(string), default(IFormatProvider));
		}
		[CanBeNull]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format)
		{
			return Convert<FromType, ToType>(typeConversionProvider, fromValue, format, default(IFormatProvider));
		}
		[CanBeNull]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			return Convert<FromType, ToType>(typeConversionProvider, fromValue, default(string), formatProvider);
		}
		[CanBeNull]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			typeConversionProvider.GetConverter<FromType, ToType>().Convert(fromValue, out var result, format, formatProvider);
			return result;
		}

		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result)
		{
			return TryConvert<FromType, ToType>(typeConversionProvider, fromValue, out result, default(string), default(IFormatProvider));
		}
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] string format)
		{
			return TryConvert<FromType, ToType>(typeConversionProvider, fromValue, out result, format, default(IFormatProvider));
		}
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] IFormatProvider formatProvider)
		{
			return TryConvert<FromType, ToType>(typeConversionProvider, fromValue, out result, default(string), formatProvider);
		}
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			return typeConversionProvider.GetConverter<FromType, ToType>().TryConvert(fromValue, out result, format, formatProvider);
		}

		[NotNull]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue)
		{
			return ConvertToString<FromType>(typeConversionProvider, fromValue, default(string), default(IFormatProvider));
		}
		[NotNull]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format)
		{
			return ConvertToString<FromType>(typeConversionProvider, fromValue, format, default(IFormatProvider));
		}
		[NotNull]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			return ConvertToString<FromType>(typeConversionProvider, fromValue, default(string), formatProvider);
		}
		[NotNull]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			typeConversionProvider.GetConverter<FromType, string>().Convert(fromValue, out var result, format, formatProvider);
			return result ?? string.Empty;
		}
	}
}
