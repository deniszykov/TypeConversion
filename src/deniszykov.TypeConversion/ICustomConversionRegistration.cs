
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Registration of a custom conversion that can be applied to an <see cref="ICustomConversionRegistry"/>.
	/// Implementations are passed to <see cref="TypeConversionProvider"/> via DI to expose additional conversions.
	/// </summary>
	[PublicAPI]
	public interface ICustomConversionRegistration
	{
		/// <summary>
		/// Apply this registration to the specified <paramref name="registry"/>, registering one or more conversion functions on it.
		/// </summary>
		/// <param name="registry">Target registry which will receive the conversion registration. Typically the active <see cref="TypeConversionProvider"/>.</param>
		void Register(ICustomConversionRegistry registry);
	}
}
