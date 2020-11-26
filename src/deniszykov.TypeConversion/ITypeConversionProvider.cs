using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public interface ITypeConversionProvider
	{
		[NotNull]
		IConverter<FromType, ToType> GetConverter<FromType, ToType>();

		[NotNull]
		IConverter GetConverter([NotNull]Type fromType, [NotNull]Type toType);
	}
}