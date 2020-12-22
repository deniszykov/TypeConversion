using System;

namespace deniszykov.TypeConversion
{
	internal interface IEnumConversionInfo
	{
		Delegate ToNumber { get; }
		Delegate FromNumber { get; set; }
		TypeCode UnderlyingTypeCode { get; }
		Type UnderlyingType { get; }
		Type Type { get; }
	}
}