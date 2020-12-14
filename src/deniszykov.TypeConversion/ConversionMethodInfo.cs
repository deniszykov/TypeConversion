using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public class ConversionMethodInfo : IComparable<ConversionMethodInfo>
	{
		[NotNull]
		public readonly MethodBase Method;
		[NotNull, ItemNotNull]
		internal readonly ParameterInfo[] Parameters;
		[NotNull]
		internal readonly Type FromType;
		[NotNull]
		internal readonly Type ToType;

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
				if (fromValueParameterIndex < 0 || fromValueParameterIndex >= this.Parameters.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(fromValueParameterIndex));
				}
				else
				{
					this.FromType = this.Parameters[fromValueParameterIndex].ParameterType;
				}
			}
			else
			{
				if (fromValueParameterIndex < 0 || fromValueParameterIndex >= this.Parameters.Length)
				{
					this.FromType = methodBase.DeclaringType ?? typeof(object);
				}
				else
				{
					this.FromType = this.Parameters[fromValueParameterIndex].ParameterType;
				}
			}
			this.ToType = methodBase is MethodInfo methodInfo ? methodInfo.ReturnType : methodBase.DeclaringType ?? typeof(object);
			this.Quality = conversionQualityOverride ?? (methodBase.Name == "op_Explicit" ? ConversionQuality.Explicit :
				methodBase.Name == "op_Implicit" ? ConversionQuality.Implicit :
				methodBase is ConstructorInfo ? ConversionQuality.Constructor : ConversionQuality.Method);
		}

		/// <inheritdoc />
		public int CompareTo(ConversionMethodInfo other)
		{
			if (other == null)
			{
				return 1;
			}

			var cmp = ((int)this.Quality).CompareTo((int)other.Quality); // better quality conversion
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = (this.Method.Name.IndexOf(this.ToType.Name, StringComparison.OrdinalIgnoreCase) >= 0)
				.CompareTo(this.Method.Name.IndexOf(this.ToType.Name, StringComparison.OrdinalIgnoreCase) >= 0); // has target type in name = better
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = (this.Method.Name.IndexOf(this.FromType.Name, StringComparison.OrdinalIgnoreCase) >= 0)
				.CompareTo(this.Method.Name.IndexOf(this.FromType.Name, StringComparison.OrdinalIgnoreCase) >= 0); // has source type in name = better
			if (cmp != 0)
			{
				return cmp;
			}

			cmp = this.Parameters.Length.CompareTo(other.Parameters.Length); // more parameters (format, formatProvider) = better
			if (cmp != 0)
			{
				return cmp;
			}

			return other.Method.Name.Length.CompareTo(this.Method.Name.Length); // shorter name = better 
		}

		/// <inheritdoc />
		public override string ToString() => $"From: {this.FromType.Name}, To: {this.ToType.Name}, Method: {this.Method.Name} ({string.Join(", ", this.Parameters.Select(p => p.Name))}), Quality: {this.Quality}";

	}
}