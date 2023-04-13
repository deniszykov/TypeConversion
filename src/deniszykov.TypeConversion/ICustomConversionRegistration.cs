
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	[PublicAPI]
	public interface ICustomConversionRegistration
	{
		void Register(ICustomConversionRegistry registry);
	}
}
