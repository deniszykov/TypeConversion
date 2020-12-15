namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Type of parameter in conversion method.
	/// </summary>
	public enum ConversionParameterType
	{
		/// <summary>
		/// Original value to convert. If this parameter is not present then value is <value>this</value> for current method.
		/// </summary>
		Value,
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
