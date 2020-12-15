using System;
using System.Globalization;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Converter used to transform value of type <see cref="FromType"/> to value of <see cref="ToType"/>.
	/// </summary>
	public interface IConverter
	{
		/// <summary>
		/// Description of method used for this conversion.
		/// </summary>
		[NotNull]
		ConversionDescriptor Descriptor { get; }
		/// <summary>
		/// Source type of conversion.
		/// </summary>
		[NotNull]
		Type FromType { get; }
		/// <summary>
		/// Destination type of conversion.
		/// </summary>
		[NotNull]
		Type ToType { get; }

		/// <summary>
		/// Covert <paramref name="value"/> from <see cref="FromType"/> to <see cref="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		void Convert(object value, out object result, string format = null, IFormatProvider formatProvider = null);

		/// <summary>
		/// Tries to covert <paramref name="value"/> from <see cref="FromType"/> to <see cref="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value or null if conversion fails.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		bool TryConvert(object value, out object result, string format = null, IFormatProvider formatProvider = null);
	}

	public interface IConverter<in FromType, ToType> : IConverter
	{

		/// <summary>
		/// Covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		void Convert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);

		/// <summary>
		/// Tries to covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value or <value>default</value> if conversion fails.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		bool TryConvert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);
	}
}