using System;
using System.Runtime.ExceptionServices;

namespace deniszykov.TypeConversion
{
	internal static class ExceptionExtensions
	{
		public static ExceptionT Rethrow<ExceptionT>(this ExceptionT exception) where ExceptionT : Exception
		{
#if !NET45
			ExceptionDispatchInfo.Capture(exception).Throw();
#endif
			return exception;

		}
	}
}
