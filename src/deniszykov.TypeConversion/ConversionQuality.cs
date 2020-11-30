namespace deniszykov.TypeConversion
{
	public enum ConversionQuality
	{
		/// <summary>
		/// No conversion method is found. Trying to invoke conversion with this quality will cause <see cref="System.InvalidOperationException"/>.
		/// </summary>
		None,
		/// <summary>
		/// Conversion via Constructor accepting appropriate type.
		/// </summary>
		Constructor,
		/// <summary>
		/// Conversion via System.ComponentModel.TypeConverter instance.
		/// </summary>
		TypeConverter,
		/// <summary>
		/// Conversion via method on type (Parse, ToXXX, FromXXX)
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
