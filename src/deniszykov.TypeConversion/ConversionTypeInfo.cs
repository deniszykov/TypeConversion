using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace deniszykov.TypeConversion
{
	public static class ConversionTypeInfo
	{
		private static int LastFromIndex = -1;

		// ReSharper disable StaticMemberInGenericType, UnusedTypeParameter
		public static class FromType<FromT>
		{
			private static int LastToIndex = -1;

			public static class ToType<ToT>
			{
				public static readonly int ToIndex = Interlocked.Increment(ref LastToIndex);

				private static readonly ConversionMethodInfo[] ConvertMethods; // from FromT type to ToT type

				static ToType()
				{
					var methods = new List<ConversionMethodInfo>();
					foreach (var convertMethod in ConvertToMethods)
					{
						if (convertMethod.ToType == typeof(ToT))
						{
							methods.Add(convertMethod);
						}
					}
					foreach(var convertMethod in FromType<ToT>.ConvertFromMethods)
					{
						if (convertMethod.FromType == typeof(FromT))
						{
							methods.Add(convertMethod);
						}
					}

					ConvertMethods = methods.ToArray();
				}
			}

			public static readonly int FromIndex = Interlocked.Increment(ref LastFromIndex);

			private static readonly ConversionMethodInfo[] ConvertFromMethods; // from OTHER type to FromT type
			private static readonly ConversionMethodInfo[] ConvertToMethods; // from FromT type to OTHER type

			static FromType()
			{

			}
		}
		// ReSharper restore StaticMemberInGenericType, UnusedTypeParameter

	}
}
