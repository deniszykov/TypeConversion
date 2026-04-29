using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Conversion registration which registers a custom conversion function from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/>.
	/// Pass an instance of this class to <see cref="TypeConversionProvider"/>'s constructor (typically via DI) to make the conversion available.
	/// </summary>
	/// <typeparam name="FromTypeT">Conversion source type.</typeparam>
	/// <typeparam name="ToTypeT">Conversion destination type.</typeparam>
	[PublicAPI]
	public sealed class CustomConversion<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		FromTypeT,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		ToTypeT
	> : ICustomConversionRegistration
	{
		private readonly Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc;
		private readonly Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>? safeConversionFunc;
		private readonly ConversionQuality quality;

		/// <summary>
		/// Constructor of <see cref="CustomConversion{FromTypeT,ToTypeT}"/>.
		/// </summary>
		/// <param name="conversionFunc">Conversion function which takes value of <typeparamref name="FromTypeT"/> and converts it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="quality">Conversion quality used to rank this conversion against alternatives. Defaults to <see cref="ConversionQuality.Custom"/>.</param>
		public CustomConversion(Func<FromTypeT, string?, IFormatProvider?, ToTypeT> conversionFunc, ConversionQuality quality = ConversionQuality.Custom)
			: this(conversionFunc, null, quality)
		{
		}
		/// <summary>
		/// Constructor of <see cref="CustomConversion{FromTypeT,ToTypeT}"/> with a separate "safe" (non-throwing) conversion function.
		/// </summary>
		/// <param name="conversionFunc">Conversion function which takes value of <typeparamref name="FromTypeT"/> and converts it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>.</param>
		/// <param name="safeConversionFunc">Conversion function which takes value of <typeparamref name="FromTypeT"/> and tries to convert it to <typeparamref name="ToTypeT"/> using specified format and <see cref="IFormatProvider"/>. Returned <see cref="KeyValuePair{ToTypeT,Boolean}.Value"/> indicates success.</param>
		/// <param name="quality">Conversion quality used to rank this conversion against alternatives. Defaults to <see cref="ConversionQuality.Custom"/>.</param>
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
