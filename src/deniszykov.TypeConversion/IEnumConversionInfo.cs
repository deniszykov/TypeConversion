using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	[PublicAPI]
	internal interface IEnumConversionInfo
	{
		Delegate ToNumber { get; }
		Delegate FromNumber { get; set; }
		TypeCode UnderlyingTypeCode { get; }
		Type UnderlyingType { get; }
		Type Type { get; }
	}
}