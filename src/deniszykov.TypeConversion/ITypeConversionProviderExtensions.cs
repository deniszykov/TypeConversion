

using System;
using System.Globalization;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Extensions method for fast conversion with <see cref="ITypeConversionProvider"/> class.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class ITypeConversionProviderExtensions
	{
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified default format and format provider (<see cref="CultureInfo.InvariantCulture"/>).
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified <paramref name="format"/> and default format provider (<see cref="CultureInfo.InvariantCulture"/>).
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] string format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			converter.Convert(fromValue, out var result, format, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, formatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static object Convert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result;
		}

		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using default format and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="result">Converted to <paramref name="toType"/> value or null.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified <paramref name="format"/> and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="result">Converted to <paramref name="toType"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] string format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			return converter.TryConvert(fromValue, out result, format, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="result">Converted to <paramref name="toType"/> value or null.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, formatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <paramref name="fromType"/> to <paramref name="toType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromType">Convert from type.</param>
		/// <param name="toType">Convert to type.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <paramref name="fromType"/> type.</param>
		/// <param name="result">Converted to <paramref name="toType"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert([NotNull] this ITypeConversionProvider typeConversionProvider, [NotNull] Type fromType, [NotNull] Type toType, [CanBeNull] object fromValue, [CanBeNull] out object result, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			return converter.TryConvert(fromValue, out result, format, formatProvider);
		}

		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using default format and default format provider.
		/// </summary>
		/// <typeparam name="FromType"></typeparam>
		/// <typeparam name="ToType"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and default format provider.
		/// </summary>
		/// <typeparam name="FromType"></typeparam>
		/// <typeparam name="ToType"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			converter.Convert(fromValue, out var result, format, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using default format and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType"></typeparam>
		/// <typeparam name="ToType"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, formatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[CanBeNull, MustUseReturnValue, Pure]
		public static ToType Convert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result;
		}

		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using default format and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToType"/> value or null.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToType"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] string format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			return converter.TryConvert(fromValue, out result, format, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using default format and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToType"/> value or null.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, formatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToType"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromType, ToType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] out ToType result, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, ToType>();
			return converter.TryConvert(fromValue, out result, format, formatProvider);
		}

		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <see cref="string"/> using default format and default format provider.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[NotNull, MustUseReturnValue, Pure]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, string>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <see cref="string"/> using specified <paramref name="format"/> and default format provider.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[NotNull, MustUseReturnValue, Pure]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, string>();
			converter.Convert(fromValue, out var result, format, converter.Descriptor.DefaultFormatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <see cref="string"/> using default format and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[NotNull, MustUseReturnValue, Pure]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, string>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, formatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromType"/> to <see cref="string"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[NotNull, MustUseReturnValue, Pure]
		public static string ConvertToString<FromType>([NotNull] this ITypeConversionProvider typeConversionProvider, [CanBeNull] FromType fromValue, [CanBeNull] string format, [CanBeNull] IFormatProvider formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromType, string>();
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result ?? string.Empty;
		}
	}
}
