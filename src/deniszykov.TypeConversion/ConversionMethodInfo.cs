using System;
using System.Linq;
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

		public ConversionMethodInfo(
			[NotNull] MethodBase methodBase,
			int fromValueParameterIndex,
			[CanBeNull] ParameterInfo[] methodParameters = null,
			[CanBeNull] ConversionQuality? conversionQualityOverride = null)
		{
			if (methodBase == null) throw new ArgumentNullException(nameof(methodBase));

			this.Parameters = methodParameters ?? methodBase.GetParameters();
			this.Method = methodBase;
			if (methodBase.IsStatic)
			{
				if (fromValueParameterIndex < 0 || fromValueParameterIndex >= this.Parameters.Length) throw new ArgumentOutOfRangeException(nameof(fromValueParameterIndex));
				this.FromType = this.Parameters[fromValueParameterIndex].ParameterType;
			}
			else
			{
				this.FromType = methodBase.DeclaringType ?? typeof(object);
			}
			this.ToType = methodBase is MethodInfo methodInfo ? methodInfo.ReturnType : methodBase.DeclaringType ?? typeof(object);
			this.Quality = conversionQualityOverride ?? (methodBase.Name == "op_Explicit" ? ConversionQuality.Explicit :
				methodBase.Name == "op_Implicit" ? ConversionQuality.Implicit :
				methodBase is ConstructorInfo ? ConversionQuality.Constructor : ConversionQuality.Method);
		}

		internal static ConversionMethodInfo ChooseByQuality(ConversionMethodInfo x, ConversionMethodInfo y)
		{
			switch (Math.Sign(Compare(x, y)))
			{
				case 1: return x;
				case -1: return y;
				case 0:
				default: return y;
			}
		}
		internal static int Compare(ConversionMethodInfo x, ConversionMethodInfo y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			else if (x != null && y == null)
			{
				return 1;
			}
			else if (x == null)
			{
				return -1;
			}

			var cmp = ((int)x.Quality).CompareTo((int)y.Quality); // better quality conversion
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = (x.Method.Name.IndexOf(x.ToType.Name, StringComparison.OrdinalIgnoreCase) >= 0)
				.CompareTo(x.Method.Name.IndexOf(x.ToType.Name, StringComparison.OrdinalIgnoreCase) >= 0); // has target type in name = better
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = (x.Method.Name.IndexOf(x.FromType.Name, StringComparison.OrdinalIgnoreCase) >= 0)
				.CompareTo(x.Method.Name.IndexOf(x.FromType.Name, StringComparison.OrdinalIgnoreCase) >= 0); // has source type in name = better
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = x.Parameters.Length.CompareTo(y.Parameters.Length); // more parameters (format, formatProvider) = better
			if (cmp != 0)
			{
				return cmp;
			}

			return y.Method.Name.Length.CompareTo(x.Method.Name.Length); // shorter name = better 
		}

		/// <inheritdoc />
		public override string ToString() => $"From: {this.FromType.Name}, To: {this.ToType.Name}, Method: {this.Method.Name} ({string.Join(", ", this.Parameters.Select(p => p.Name))}), Quality: {this.Quality}";
	}
}