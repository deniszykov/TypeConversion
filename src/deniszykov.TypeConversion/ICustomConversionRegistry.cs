using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Container containing conversion function registration. Implementations are <see cref="TypeConversionProvider"/> and <see cref="TypeConversionProviderOptions"/>.
	/// </summary>
	[PublicAPI]
	public interface ICustomConversionRegistry
	{
		/// <summary>
		/// Register conversion between <typeparamref name="FromTypeT"/> and <typeparamref name="ToTypeT"/> via specified <paramref name="conversionFunc"/>.
		/// </summary>
		/// <param name="conversionFunc">Conversion function which take value of <typeparamref name="FromTypeT"/> and converts it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="quality">Conversion quality for this conversion. Default to <see cref="ConversionQuality.Custom"/></param>
		void RegisterConversion<FromTypeT, ToTypeT>(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc, ConversionQuality quality);

		/// <summary>
		/// Register conversion between <typeparamref name="FromTypeT"/> and <typeparamref name="ToTypeT"/> via specified <paramref name="conversionFunc"/>.
		/// </summary>
		/// <param name="conversionFunc">Conversion function which take value of <typeparamref name="FromTypeT"/> and converts it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="safeConversionFunc">Conversion function which take value of <typeparamref name="FromTypeT"/> and try to convert it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="quality">Conversion quality for this conversion. Default to <see cref="ConversionQuality.Custom"/></param>
		void RegisterConversion<FromTypeT, ToTypeT>(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc, Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>? safeConversionFunc, ConversionQuality quality);
	}
}
