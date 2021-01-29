using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Container containing conversion function registration. Implementations are <see cref="TypeConversionProvider"/> and <see cref="TypeConversionProviderConfiguration"/>.
	/// </summary>
	public interface ICustomConversionRegistry
	{
		/// <summary>
		/// Register conversion between <typeparamref name="FromType"/> and <typeparamref name="ToType"/> via specified <paramref name="conversionFunc"/>.
		/// </summary>
		/// <param name="conversionFunc">Conversion function which take value of <typeparamref name="FromType"/> and converts it to <typeparamref name="ToType"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="quality">Conversion quality for this conversion. Default to <see cref="ConversionQuality.Custom"/></param>
		void RegisterConversion<FromType, ToType>([NotNull] Func<FromType, string, IFormatProvider, ToType> conversionFunc, ConversionQuality quality);
	}
}
