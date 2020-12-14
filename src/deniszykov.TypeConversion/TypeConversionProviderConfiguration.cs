using System;
using System.Runtime.Serialization;

namespace deniszykov.TypeConversion
{
	[DataContract]
	public class TypeConversionProviderConfiguration
	{
		[DataMember]
		public bool IsAotRuntime;
		[DataMember]
		public string DefaultFormatProviderCultureName;
		[DataMember]
		public ConverterOptions ConverterOptions;
		[DataMember]
		public ConversionMethodSelectionStrategy ConversionMethodSelectionStrategy;

		public IFormatProvider DefaultFormatProvider;
	}
}
