using System;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public class ConversionInfo
	{
		[NotNull]
		public readonly ConversionMethodInfo Method;
		[CanBeNull]
		public readonly string DefaultFormat;

		[NotNull]
		public readonly Delegate Conversion; // Func<FromType, string, IFormatProvider, ToType>
		[CanBeNull]
		public readonly Delegate SafeConversion; // Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>

		public ConversionInfo([NotNull] ConversionMethodInfo method, [CanBeNull] string defaultFormat, [NotNull] Delegate conversion, [CanBeNull] Delegate safeConversion)
		{
			if (method == null) throw new ArgumentNullException(nameof(method));
			if (conversion == null) throw new ArgumentNullException(nameof(conversion));

			this.Method = method;
			this.DefaultFormat = defaultFormat;
			this.Conversion = conversion;
			this.SafeConversion = safeConversion;
		}

		/// <inheritdoc />
		public override string ToString() => $"Method: ({this.Method}), Default Format: {this.DefaultFormat}";
	}

}