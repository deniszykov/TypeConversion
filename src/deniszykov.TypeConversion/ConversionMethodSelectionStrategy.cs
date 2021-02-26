using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Strategy to select conversion method from alternatives.
	/// </summary>
	[PublicAPI]
	public enum ConversionMethodSelectionStrategy
	{
		/// <summary>
		/// Choose method with most additional parameters like 'format', 'formatProvider' even if 'format' and 'formatProvider' is passed as <value>null</value>.
		/// </summary>
		MostSpecificMethod,
		/// <summary>
		/// Choose method depending of on passed into <see cref="IConverter.Convert"/> parameters.
		/// If 'format' is passed, choose method with 'format' parameter, if 'formatProvider' is passed, choose method with 'formatParameter' parameter etc.
		/// </summary>
		MostFittingMethod,
		/// <summary>
		/// Default value. Set to <see cref="MostFittingMethod"/> due less runtime errors.
		/// </summary>
		Default = MostFittingMethod,
	}
}