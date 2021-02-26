using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Options for <see cref="IConverter"/> and <see cref="ITypeConversionProvider"/> types.
	/// </summary>
	[Flags]
	[PublicAPI]
	public enum ConversionOptions
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
		/// Optimize performance with <see cref="Expression{TDelegate}.Compile()"/> to generate dynamic code.
		/// Disable if runtime doesn't support dynamic code execution. For example on Unity's WebGL, Playstation, or Mono with --aot flag.
		/// </summary>
		OptimizeWithExpressions = 0x1 << 2,
		/// <summary>
		/// Optimize performance with usage of generics.
		/// Disable if runtime doesn't support JIT compilation of new code. For example on Unity's WebGL, Playstation, or Mono with --aot flag.
		/// </summary>
		OptimizeWithGenerics = 0x1 << 3,
		/// <summary>
		/// Converter tries to cast value to desired type skipping conversion function.
		/// This optimization could give some performance gain in exchange for possible errors due not calling conversion function.
		/// </summary>
		FastCast = 0x1 << 4,
		/// <summary>
		/// Default option. Set to <see cref="UseDefaultFormatProviderIfNotSpecified"/> because this will lead to predictable conversion on different platforms and locales, <see cref="OptimizeWithExpressions"/> and <see cref="OptimizeWithGenerics"/> optimizations.
		/// </summary>
		Default = UseDefaultFormatProviderIfNotSpecified | OptimizeWithExpressions | OptimizeWithGenerics
	}
}
