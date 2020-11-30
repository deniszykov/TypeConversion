using System.Runtime.Serialization;

namespace deniszykov.TypeConversion
{
	[DataContract]
	public class TypeConversionProviderConfiguration
	{
		[DataMember]
		public bool IsAotRuntime;
	}
}
