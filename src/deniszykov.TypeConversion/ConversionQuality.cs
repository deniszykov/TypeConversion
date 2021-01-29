namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Quality of conversion method. Quality is defined by purpose and performance.
	/// Therefore, the explicit/implicit cast operators are the highest quality way of type conversion, second only to the native conversion (type casting and boxing).
	/// </summary>
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
