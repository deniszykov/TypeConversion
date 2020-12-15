using System;
using System.Runtime.Serialization;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Configuration for <see cref="TypeConversionProvider"/> class.
	/// </summary>
	[DataContract]
	public class TypeConversionProviderConfiguration
	{
		/// <summary>
		/// Set to true if runtime doesn't support dynamic code execution like dynamic method and LINQ expressions. For example on Unity's WebGL, Playstation, or Mono with --aot flag.
		/// </summary>
		[DataMember]
		public bool IsAotRuntime;
		/// <summary>
		/// Name of default format provider to set <see cref="ConversionDescriptor.DefaultFormatProvider"/>.
		/// Application of <see cref="ConversionDescriptor.DefaultFormatProvider"/> is controlled by <see cref="ConverterOptions"/> configuration parameter.
		/// If both <see cref="DefaultFormatProvider"/> and <see cref="DefaultFormatProviderCultureName"/> are set then <see cref="DefaultFormatProvider"/> is used.
		/// </summary>
		[DataMember]
		public string DefaultFormatProviderCultureName;
		/// <summary>
		/// Option to set on created <see cref="Converter{FromType,ToType}"/> instances.
		/// </summary>
		[DataMember]
		public ConverterOptions ConverterOptions;
		/// <summary>
		/// Conversion method selection strategy determines which method to choose when multiple are available.
		/// </summary>
		[DataMember]
		public ConversionMethodSelectionStrategy ConversionMethodSelectionStrategy;

		/// <summary>
		/// Default format provider to set <see cref="ConversionDescriptor.DefaultFormatProvider"/>.
		/// Application of <see cref="ConversionDescriptor.DefaultFormatProvider"/> is controlled by <see cref="ConverterOptions"/> configuration parameter.
		/// If both <see cref="DefaultFormatProvider"/> and <see cref="DefaultFormatProviderCultureName"/> are set then <see cref="DefaultFormatProvider"/> is used.
		/// </summary>
		public IFormatProvider DefaultFormatProvider;
	}
}
