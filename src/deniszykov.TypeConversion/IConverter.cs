using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public interface IConverter
	{
		[NotNull]
		ConversionInfo Info { get; }
		[NotNull]
		Type FromType { get; }
		[NotNull]
		Type ToType { get; }

		void Convert(object value, out object result, string format = null, IFormatProvider formatProvider = null);
		bool TryConvert(object value, out object result, string format = null, IFormatProvider formatProvider = null);
	}

	public interface IConverter<in FromType, ToType> : IConverter
	{
		void Convert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);
		bool TryConvert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);
	}
}