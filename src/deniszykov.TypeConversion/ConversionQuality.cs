namespace deniszykov.TypeConversion
{
	public enum ConversionQuality
	{
		/// <summary>
		/// Constructor accepting appropriate type.
		/// </summary>
		Constructor,
		/// <summary>
		/// <see cref="System.ComponentModel.TypeConverter"/> instance.
		/// </summary>
		TypeConverter,
		/// <summary>
		/// Conversion method on type (Parse, ToXXX, FromXXX)
		/// </summary>
		Method,
		/// <summary>
		/// Explicit conversion operator.
		/// </summary>
		Explicit,
		/// <summary>
		/// Implicit conversion operator.
		/// </summary>
		Implicit,
		/// <summary>
		/// Runtime provided conversion method for build-in types.
		/// </summary>
		Native,
		/// <summary>
		/// User-provided conversion function.
		/// </summary>
		Custom
	}
}
