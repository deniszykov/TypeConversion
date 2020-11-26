using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public class ConversionInfo
	{
		[NotNull]
		public readonly ConversionMethodInfo Method;
		[NotNull]
		public readonly string DefaultFormat;

		[NotNull]
		public readonly Delegate Conversion; // Func<FromType, string, IFormatProvider, ToType>
		[NotNull]
		public readonly Delegate SafeConversion; // Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>

	}

}