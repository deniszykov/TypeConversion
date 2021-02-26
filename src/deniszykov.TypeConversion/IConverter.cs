using System;
using System.Globalization;
using JetBrains.Annotations;

#if NETCOREAPP3_0 || NETSTANDARD2_1
using AllowNull = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
using MaybeNull = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
#else
using AllowNull = JetBrains.Annotations.CanBeNullAttribute;
using MaybeNull = JetBrains.Annotations.CanBeNullAttribute;
#endif

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Converter used to transform value of type <see cref="FromType"/> to value of <see cref="ToType"/>.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public interface IConverter
	{
		/// <summary>
		/// Description of method used for this conversion.
		/// </summary>
		ConversionDescriptor Descriptor { get; }
		/// <summary>
		/// Source type of conversion.
		/// </summary>
		Type FromType { get; }
		/// <summary>
		/// Destination type of conversion.
		/// </summary>
		Type ToType { get; }

		/// <summary>
		/// Covert <paramref name="value"/> from <see cref="FromType"/> to <see cref="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		void Convert(object? value, out object? result, string? format = null, IFormatProvider? formatProvider = null);

		/// <summary>
		/// Tries to covert <paramref name="value"/> from <see cref="FromType"/> to <see cref="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value or null if conversion fails.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		bool TryConvert(object? value, out object? result, string? format = null, IFormatProvider? formatProvider = null);
	}

	/// <summary>
	/// Provides type conversion methods from <typeparamref name="FromType"/> to <typeparamref name="ToType"/>.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public interface IConverter<in FromType, ToType> : IConverter
	{
		/// <summary>
		/// Covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		void Convert([AllowNull] FromType value, [MaybeNull] out ToType result, string? format = null, IFormatProvider? formatProvider = null
		);

		/// <summary>
		/// Tries to covert <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="value">A value to convert.</param>
		/// <param name="result">A converted value or <value>default</value> if conversion fails.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		bool TryConvert([AllowNull] FromType value, [MaybeNull] out ToType result, string? format = null, IFormatProvider? formatProvider = null
		);
	}
}