using System;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public class ConversionMethodInfo
	{
		[NotNull]
		public readonly MethodBase Method;
		[NotNull, ItemNotNull]
		public readonly ParameterInfo[] Parameters;
		[NotNull]
		public readonly Type FromType;
		[NotNull]
		public readonly Type ToType;
		public readonly ConversionQuality Quality;

		internal ConversionMethodInfo([NotNull] MethodBase methodBase, ref ParameterInfo[] methodParameters)
		{
			if (methodBase == null) throw new ArgumentNullException(nameof(methodBase));

			methodParameters ??= methodBase.GetParameters();
			this.Parameters = methodParameters;
			this.Method = methodBase;
			this.FromType = (methodBase.IsStatic ? methodParameters[0].ParameterType : methodBase.DeclaringType) ?? typeof(object);
			this.ToType = methodBase is MethodInfo methodInfo ? methodInfo.ReturnType : methodBase.DeclaringType ?? typeof(object);
			this.Quality = methodBase.Name == "op_Explicit" ? ConversionQuality.Explicit :
				methodBase.Name == "op_Implicit" ? ConversionQuality.Implicit :
				methodBase is ConstructorInfo ? ConversionQuality.Constructor : ConversionQuality.Method;
		}

		internal static ConversionMethodInfo ChooseByQuality(ConversionMethodInfo x, ConversionMethodInfo y)
		{
			if (x == null && y == null)
			{
				return null;
			}
			else if (x != null && y == null)
			{
				return x;
			}
			else if (x == null)
			{
				return y;
			}

			if (x.Quality >= y.Quality)
			{
				return x;
			}
			else if (x.Parameters.Length > y.Parameters.Length) // more parameter = more precise conversion because of format and format provider
			{
				return x;
			}
			else
			{
				return y;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("From: " + this.FromType.Name + ", To: " + this.ToType.Name + ", Method: " + this.Method.Name + ", Quality:" + this.Quality);
		}
	}
}