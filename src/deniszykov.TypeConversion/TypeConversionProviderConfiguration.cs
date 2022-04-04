using System;
using System.Globalization;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Configuration for <see cref="TypeConversionProvider"/> class.
	/// </summary>
	[DataContract]
	[PublicAPI]
	public class TypeConversionProviderConfiguration : ICustomConversionRegistry
	{
		/// <summary>
		/// Name of default format provider to set <see cref="ConversionDescriptor.DefaultFormatProvider"/>.
		/// Application of <see cref="ConversionDescriptor.DefaultFormatProvider"/> is controlled by <see cref="Options"/> configuration parameter.
		/// If both <see cref="DefaultFormatProvider"/> and <see cref="DefaultFormatProviderCultureName"/> are set then <see cref="DefaultFormatProvider"/> is used.
		/// </summary>
		[DataMember]
		public string? DefaultFormatProviderCultureName;
		/// <summary>
		/// Options used by <see cref="Converter{FromType,ToType}"/> and <see cref="TypeConversionProvider"/> types.
		/// </summary>
		[DataMember]
		public ConversionOptions Options;
		/// <summary>
		/// Conversion method selection strategy determines which method to choose when multiple are available.
		/// </summary>
		[DataMember]
		public ConversionMethodSelectionStrategy ConversionMethodSelectionStrategy;

		/// <summary>
		/// Default format provider to set <see cref="ConversionDescriptor.DefaultFormatProvider"/>. Default to <see cref="CultureInfo.InvariantCulture"/>.
		/// Application of <see cref="ConversionDescriptor.DefaultFormatProvider"/> is controlled by <see cref="Options"/> configuration parameter.
		/// If both <see cref="DefaultFormatProvider"/> and <see cref="DefaultFormatProviderCultureName"/> are set then <see cref="DefaultFormatProvider"/> is used.
		/// </summary>
		[IgnoreDataMember]
		public IFormatProvider? DefaultFormatProvider;

		public TypeConversionProviderConfiguration()
		{
			this.Options = ConversionOptions.Default;
			this.ConversionMethodSelectionStrategy = ConversionMethodSelectionStrategy.Default;
		}

		/// <summary>
		/// Delegate which could be applied to <see cref="ICustomConversionRegistry"/> to register all custom conversions which declared with <see cref="RegisterConversion{FromType,ToType}"/>.
		/// </summary>
		[IgnoreDataMember]
		[Obsolete("Please pass instance of 'CustomConversion<FromTypeT, ToTypeT>' in TypeConversionProvider's constructor for registering custom conversions.")]
		public Action<ICustomConversionRegistry>? CustomConversionRegistrationCallback;

		/// <inheritdoc />
		[Obsolete("Please pass instance of 'CustomConversion<FromTypeT, ToTypeT>' in TypeConversionProvider's constructor for registering custom conversions.")]
		public void RegisterConversion<FromTypeT, ToTypeT>(Func<FromTypeT, string, IFormatProvider, ToTypeT> conversionFunc, ConversionQuality quality = ConversionQuality.Custom)
		{
			var registration = new Action<ICustomConversionRegistry>(provider =>
			{
				provider.RegisterConversion(conversionFunc, quality);
			});
			if (this.CustomConversionRegistrationCallback == null)
			{
				this.CustomConversionRegistrationCallback = registration;
			}
			else
			{
				this.CustomConversionRegistrationCallback += registration;
			}
		}
	}
}
