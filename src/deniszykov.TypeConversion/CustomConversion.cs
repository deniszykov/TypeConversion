using System;
using System.Reflection;

namespace deniszykov.TypeConversion
{
	public sealed class CustomConversion<FromTypeT, ToTypeT> : ICustomConversionRegistration
	{
		private readonly Func<FromTypeT, string, IFormatProvider, ToTypeT> conversionFunc;
		private readonly ConversionQuality quality;

		public CustomConversion(Func<FromTypeT, string, IFormatProvider, ToTypeT> conversionFunc, ConversionQuality quality = ConversionQuality.Custom)
		{
			if (conversionFunc == null) throw new ArgumentNullException(nameof(conversionFunc));

			this.conversionFunc = conversionFunc;
			this.quality = quality;
		}

		/// <inheritdoc />
		public void Register(ICustomConversionRegistry registry)
		{
			if (registry == null) throw new ArgumentNullException(nameof(registry));

			registry.RegisterConversion(this.conversionFunc, this.quality);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.conversionFunc.GetMethodInfo()}, Quality: {this.quality}";
		}
	}
}
