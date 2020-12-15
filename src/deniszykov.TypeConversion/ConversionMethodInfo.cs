using System;
using System.Collections.ObjectModel;
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
		public readonly ReadOnlyCollection<ParameterInfo> Parameters;
		[NotNull]
		public readonly ReadOnlyCollection<ConversionParameterType> ConversionParameterTypes;
		[NotNull]
		internal readonly Type FromType;
		[NotNull]
		internal readonly Type ToType;

		public readonly ConversionQuality Quality;

		public ConversionMethodInfo(
			[NotNull] MethodBase methodBase,
			[NotNull] ParameterInfo[] parameters,
			[NotNull] ConversionParameterType[] conversionParameterTypes,
			[CanBeNull] ConversionQuality? conversionQualityOverride = null)
		{
			if (methodBase == null) throw new ArgumentNullException(nameof(methodBase));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));
			if (conversionParameterTypes == null) throw new ArgumentNullException(nameof(conversionParameterTypes));

			this.Parameters = new ReadOnlyCollection<ParameterInfo>(parameters);
			this.ConversionParameterTypes = new ReadOnlyCollection<ConversionParameterType>(conversionParameterTypes);
			this.Method = methodBase;

			var fromValueParameterIndex = this.ConversionParameterTypes.IndexOf(ConversionParameterType.Value);
			if (methodBase.IsStatic)
			{
				if (fromValueParameterIndex < 0)
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
				if (fromValueParameterIndex < 0)
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

			cmp = this.Parameters.Count.CompareTo(other.Parameters.Count); // more parameters (format, formatProvider) = better
			if (cmp != 0)
			{
				return cmp;
			}

			return other.Method.Name.Length.CompareTo(this.Method.Name.Length); // shorter name = better 
		}

		internal static ConversionMethodInfo FromNativeConversion(Delegate conversionFn, [CanBeNull] ConversionQuality? conversionQualityOverride = null)
		{
			if (conversionFn == null) throw new ArgumentNullException(nameof(conversionFn));

			var methodInfo = conversionFn.GetMethodInfo();
			var parameters = methodInfo.GetParameters();
			var parameterTypes = new[] { ConversionParameterType.Value, ConversionParameterType.Format, ConversionParameterType.FormatProvider };

			if (parameters.Length != 3 ||
				parameters[1].ParameterType != typeof(string) ||
				parameters[2].ParameterType != typeof(IFormatProvider))
			{
				throw new InvalidOperationException("Invalid native conversion method's signature. Should be fn(value, format, formatProvider).");
			}

			return new ConversionMethodInfo(methodInfo, parameters, parameterTypes, conversionQualityOverride ?? ConversionQuality.Native);
		}

		/// <inheritdoc />
		public override string ToString() => $"From: {this.FromType.Name}, To: {this.ToType.Name}, Method: {this.Method.Name} ({string.Join(", ", this.Parameters.Select(p => p.Name))}), Quality: {this.Quality}";

	}
}