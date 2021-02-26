using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal interface IEnumConversionInfo
	{
		Delegate ToNumber { get; }
		Delegate FromNumber { get; set; }
		TypeCode UnderlyingTypeCode { get; }
		Type UnderlyingType { get; }
		Type Type { get; }
	}
}