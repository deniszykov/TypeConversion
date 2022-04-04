
using System.Globalization;
using deniszykov.TypeConversion;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace System
{
	/// <summary>
	/// This is compatibility shim for old 'TypeConvert' package.
	/// </summary>
	[PublicAPI, Obsolete("Use TypeConversionProvider or ITypeConversionProvider instead.")]
	public static class TypeConvert
	{
		public static TypeConversionProvider Default = new TypeConversionProvider();

		public const string IgnoreCaseFormat = TypeConversionProvider.IgnoreCaseFormat;
		public const string CheckedConversionFormat = TypeConversionProvider.CheckedConversionFormat;
		public const string UncheckedConversionFormat = TypeConversionProvider.UncheckedConversionFormat;

		
		/// <summary>
		/// Covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="value">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="value"/>.</returns>
		public static ToType Convert<FromType, ToType>(FromType value, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.Convert<FromType, ToType>(value, format, formatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>
		/// <typeparam name="ToType">To type.</typeparam>
		/// <param name="value">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToType"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromType, ToType>(FromType value, out ToType result, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.TryConvert(value, out result, format, formatProvider);
		}
		/// <summary>
		/// Covert <paramref name="value"/> from <typeparamref name="FromType"/> to <see cref="string"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromType">From type.</typeparam>\
		/// <param name="value">Value to convert. Value should be assignable from <typeparamref name="FromType"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="value"/> or empty string if null.</returns>
		public static string ToString<FromType>(FromType value, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.ConvertToString(value, format, formatProvider);
		}
		/// <summary>
		/// Covert <paramref name="value"/> to <paramref name="toType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="toType">Convert to type.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="value"/>.</returns>
		public static object? Convert(object? value, Type toType, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.Convert(value?.GetType() ?? typeof(object), toType, value, format, formatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="value"/> to <paramref name="toType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="toType">Convert to type.</param>
		/// <param name="value">Value to convert. Converted value will be placed in this variable.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert(ref object? value, Type toType, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.TryConvert(value?.GetType() ?? typeof(object), toType, value, out value, format, formatProvider);
		}
		/// <summary>
		/// Covert <paramref name="value"/> to <see cref="string"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="value"/> or empty string if its null.</returns>
		public static string ToString(object? value, string? format = null, IFormatProvider? formatProvider = null)
		{
			return Default.ConvertToString(value, format, formatProvider);
		}
	}
}
