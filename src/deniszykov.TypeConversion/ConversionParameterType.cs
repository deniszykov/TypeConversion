using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Type of parameter in conversion method.
	/// </summary>
	[PublicAPI]
	public enum ConversionParameterType
	{
		/// <summary>
		/// Original value to convert. If this parameter is not present then value is <value>this</value> of method.
		/// </summary>
		Value,
		/// <summary>
		/// Converted value output. If this parameter is not present then output value is result type of method.
		/// </summary>
		ConvertedValue,
		/// <summary>
		/// Format parameter. Type is always <see cref="string"/>.
		/// </summary>
		Format,
		/// <summary>
		/// Format provider parameter. Type is always <see cref="System.IFormatProvider"/>.
		/// </summary>
		FormatProvider
	}
}
