using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	[PublicAPI]
	public sealed class CustomConversion<FromTypeT, ToTypeT> : ICustomConversionRegistration
	{
		private readonly Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc;
		private readonly Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>? safeConversionFunc;
		private readonly ConversionQuality quality;

		public CustomConversion(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc, ConversionQuality quality = ConversionQuality.Custom)
			: this(conversionFunc, null, quality)
		{
		}
		public CustomConversion(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc, Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>? safeConversionFunc, ConversionQuality quality = ConversionQuality.Custom)
		{
			if (conversionFunc == null) throw new ArgumentNullException(nameof(conversionFunc));

			this.conversionFunc = conversionFunc;
			this.safeConversionFunc = safeConversionFunc;
			this.quality = quality;
		}

		/// <inheritdoc />
		public void Register(ICustomConversionRegistry registry)
		{
			if (registry == null) throw new ArgumentNullException(nameof(registry));

			if (this.safeConversionFunc != null)
			{
				registry.RegisterConversion(this.conversionFunc, this.safeConversionFunc, this.quality);
			}
			else
			{
				registry.RegisterConversion(this.conversionFunc, this.quality);
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.conversionFunc.GetMethodInfo()}, Quality: {this.quality}";
		}
	}
}
