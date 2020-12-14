using System;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Options for <see cref="IConverter"/> interface.
	/// </summary>
	[Flags]
	public enum ConverterOptions
	{
		/// <summary>
		/// No special options.
		/// </summary>
		None = 0,
		/// <summary>
		/// Replace <value>null</value> in 'format' parameter with <see cref="ConversionDescriptor.DefaultFormat"/> during conversion.
		/// </summary>
		UseDefaultFormatIfNotSpecified = 0x1 << 0,
		/// <summary>
		/// Replace <value>null</value> in 'formatProvider' parameter with <see cref="ConversionDescriptor.DefaultFormatProvider"/> during conversion.
		/// </summary>
		UseDefaultFormatProviderIfNotSpecified = 0x1 << 1,

		/// <summary>
		/// Default option. Set to <see cref="UseDefaultFormatProviderIfNotSpecified"/> because this will lead to predictable conversion on different platforms and locales.
		/// </summary>
		Default = UseDefaultFormatProviderIfNotSpecified
	}
}
