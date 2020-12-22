using System;
using System.Linq.Expressions;

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
		/// Use <see cref="Expression{TDelegate}.Compile()"/> to generate dynamic code.
		/// Disable if runtime doesn't support dynamic code execution like dynamic method and LINQ expressions. For example on Unity's WebGL, Playstation, or Mono with --aot flag.
		/// </summary>
		UseDynamicMethods = 0x1 << 2,
		/// <summary>
		/// Instantiate new generics types during runtime.
		/// Disable if runtime doesn't support JIT compilation of new code. For example on Unity's WebGL, Playstation, or Mono with --aot flag.
		/// </summary>
		InstantiateNewGenericTypes = 0x1 << 3,
		/// <summary>
		/// Converter tries to cast value to desired type skipping conversion function.
		/// This optimization could give some performance gain in exchange for possible errors due not calling conversion function.
		/// </summary>
		FastCast = 0x1 << 4,
		/// <summary>
		/// Default option. Set to <see cref="UseDefaultFormatProviderIfNotSpecified"/> because this will lead to predictable conversion on different platforms and locales, <see cref="UseDynamicMethods"/> and <see cref="InstantiateNewGenericTypes"/> optimizations.
		/// </summary>
		Default = UseDefaultFormatProviderIfNotSpecified | UseDynamicMethods | InstantiateNewGenericTypes
	}
}
