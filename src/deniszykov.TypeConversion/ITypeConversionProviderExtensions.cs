

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
	/// Extensions method for fast conversion with <see cref="ITypeConversionProvider"/> class.
	/// </summary>
	[PublicAPI]
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
		[MustUseReturnValue, Pure]
		public static object? Convert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue)
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
		[MustUseReturnValue, Pure]
		public static object? Convert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, string? format)
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
		[MustUseReturnValue, Pure]
		public static object? Convert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, IFormatProvider? formatProvider)
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
		[MustUseReturnValue, Pure]
		public static object? Convert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, string? format, IFormatProvider? formatProvider)
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
		public static bool TryConvert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, out object? result)
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
		public static bool TryConvert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, out object? result, string? format)
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
		public static bool TryConvert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, out object? result, IFormatProvider? formatProvider)
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
		public static bool TryConvert(this ITypeConversionProvider typeConversionProvider, Type fromType, Type toType, object? fromValue, out object? result, string? format, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter(fromType, toType);
			return converter.TryConvert(fromValue, out result, format, formatProvider);
		}

		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using default format and default format provider.
		/// </summary>
		/// <typeparam name="FromTypeT"></typeparam>
		/// <typeparam name="ToTypeT"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[MustUseReturnValue, Pure]
#if NETCOREAPP3_0 || NETSTANDARD2_1
		[return: MaybeNull]
#else
#endif
		public static ToTypeT? Convert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using specified <paramref name="format"/> and default format provider.
		/// </summary>
		/// <typeparam name="FromTypeT"></typeparam>
		/// <typeparam name="ToTypeT"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[MustUseReturnValue, Pure]
#if NETCOREAPP3_0 || NETSTANDARD2_1
		[return: MaybeNull]
#else
#endif
		public static ToTypeT? Convert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, string? format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue, out var result, format, converter.Descriptor.DefaultFormatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using default format and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromTypeT"></typeparam>
		/// <typeparam name="ToTypeT"></typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[MustUseReturnValue, Pure]
#if NETCOREAPP3_0 || NETSTANDARD2_1
		[return: MaybeNull]
#else
#endif
		public static ToTypeT? Convert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, formatProvider);
			return result;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromTypeT">From type.</typeparam>
		/// <typeparam name="ToTypeT">To type.</typeparam>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/>.</returns>
		[MustUseReturnValue, Pure]
#if NETCOREAPP3_0 || NETSTANDARD2_1
		[return: MaybeNull]
#else
#endif
		public static ToTypeT? Convert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result;
		}

		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using default format and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromTypeT">From type.</typeparam>
		/// <typeparam name="ToTypeT">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToTypeT"/> value or null.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, out ToTypeT? result)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using specified <paramref name="format"/> and default format provider.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromTypeT">From type.</typeparam>
		/// <typeparam name="ToTypeT">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToTypeT"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, out ToTypeT? result, string? format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			return converter.TryConvert(fromValue, out result, format, converter.Descriptor.DefaultFormatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using default format and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromTypeT">From type.</typeparam>
		/// <typeparam name="ToTypeT">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToTypeT"/> value or null.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, out ToTypeT? result, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			return converter.TryConvert(fromValue, out result, converter.Descriptor.DefaultFormat, formatProvider);
		}
		/// <summary>
		/// Tries to covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// Any exception except <see cref="InvalidCastException"/>, <see cref="FormatException"/>, <see cref="ArithmeticException"/>, <see cref="NotSupportedException"/>, <see cref="ArgumentException"/> and <see cref="InvalidTimeZoneException"/> will be re-thrown.
		/// </summary>
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <typeparam name="FromTypeT">From type.</typeparam>
		/// <typeparam name="ToTypeT">To type.</typeparam>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="result">Converted to <typeparamref name="ToTypeT"/> value or null.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert<FromTypeT, ToTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, out ToTypeT? result, string? format, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, ToTypeT>();
			return converter.TryConvert(fromValue, out result, format, formatProvider);
		}

		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <see cref="string"/> using default format and default format provider.
		/// </summary>
		/// <typeparam name="FromTypeT">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[MustUseReturnValue, Pure]
		public static string ConvertToString<FromTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, string>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, converter.Descriptor.DefaultFormatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <see cref="string"/> using specified <paramref name="format"/> and default format provider.
		/// </summary>
		/// <typeparam name="FromTypeT">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[MustUseReturnValue, Pure]
		public static string ConvertToString<FromTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, string? format)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, string>();
			converter.Convert(fromValue, out var result, format, converter.Descriptor.DefaultFormatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <see cref="string"/> using default format and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromTypeT">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[MustUseReturnValue, Pure]
		public static string ConvertToString<FromTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, string>();
			converter.Convert(fromValue, out var result, converter.Descriptor.DefaultFormat, formatProvider);
			return result ?? string.Empty;
		}
		/// <summary>
		/// Covert <paramref name="fromValue"/> from <typeparamref name="FromTypeT"/> to <see cref="string"/> using specified <paramref name="format"/> and <paramref name="formatProvider"/>.
		/// </summary>
		/// <typeparam name="FromTypeT">From type.</typeparam>\
		/// <param name="typeConversionProvider">Conversion provider instance.</param>
		/// <param name="fromValue">Value to convert. Value should be assignable from <typeparamref name="FromTypeT"/> type.</param>
		/// <param name="format">Formatting options used for conversion. Value and behaviour is conversion specific.</param>
		/// <param name="formatProvider">Localization/regional settings used for conversion. Used from/to <see cref="string"/> conversions. Default to <see cref="CultureInfo.InvariantCulture"/>.</param>
		/// <returns>Converted <paramref name="fromValue"/> or empty string if null.</returns>
		[MustUseReturnValue, Pure]
		public static string ConvertToString<FromTypeT>(this ITypeConversionProvider typeConversionProvider, FromTypeT? fromValue, string? format, IFormatProvider? formatProvider)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));

			var converter = typeConversionProvider.GetConverter<FromTypeT, string>();
			converter.Convert(fromValue, out var result, format, formatProvider);
			return result ?? string.Empty;
		}
	}
}
