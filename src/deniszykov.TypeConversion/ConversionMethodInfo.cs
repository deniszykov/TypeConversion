using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Information about .NET Method used to perform conversion.
	/// </summary>
	public class ConversionMethodInfo : IComparable<ConversionMethodInfo>
	{
		/// <summary>
		/// Method used to create conversion delegate.
		/// </summary>
		public readonly MethodBase Method;
		/// <summary>
		/// List of <see cref="Method"/>'s parameters.
		/// </summary>
		public readonly ReadOnlyCollection<ParameterInfo> Parameters;
		/// <summary>
		/// List of <see cref="Parameters"/>'s roles in conversion.
		/// </summary>
		public readonly ReadOnlyCollection<ConversionParameterType> ConversionParameterTypes;
		/// <summary>
		/// Conversion source type.
		/// </summary>
		internal readonly Type FromType;
		/// <summary>
		/// Conversion destination type.
		/// </summary>
		internal readonly Type ToType;
		/// <summary>
		/// Conversion quality with specified <see cref="Method"/>.
		/// </summary>
		public readonly ConversionQuality Quality;

		/// <summary>
		/// Return true if it is <value>bool TryParse(value, out result)</value> conversion method.
		/// </summary>
		public bool IsSafeConversion { get; }

		/// <summary>
		/// Constructor of <see cref="ConversionMethodInfo"/>.
		/// </summary>
		/// <param name="methodBase">Value for <see cref="Method"/>.</param>
		/// <param name="parameters">Value for <see cref="Parameters"/>..</param>
		/// <param name="conversionParameterTypes">Value for <see cref="ConversionParameterTypes"/>.</param>
		/// <param name="conversionQualityOverride">Override value for <see cref="Quality"/>. If not set then quality is determinate by signature.</param>
		public ConversionMethodInfo(
			 MethodBase methodBase,
			 ParameterInfo[] parameters,
			 ConversionParameterType[] conversionParameterTypes,
			 ConversionQuality? conversionQualityOverride = null)
		{
			if (methodBase == null) throw new ArgumentNullException(nameof(methodBase));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));
			if (conversionParameterTypes == null) throw new ArgumentNullException(nameof(conversionParameterTypes));

			this.Parameters = new ReadOnlyCollection<ParameterInfo>(parameters);
			this.ConversionParameterTypes = new ReadOnlyCollection<ConversionParameterType>(conversionParameterTypes);
			this.Method = methodBase;
			this.IsSafeConversion = this.ConversionParameterTypes.Any(parameterType => parameterType == ConversionParameterType.ConvertedValue);

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

			var resultValueParameterIndex = this.ConversionParameterTypes.IndexOf(ConversionParameterType.ConvertedValue);
			if (resultValueParameterIndex >= 0)
			{
				// ConvertedValue always 'by-ref out' parameter
				this.ToType = this.Parameters[resultValueParameterIndex].ParameterType.GetElementType()!;
			}
			else
			{
				this.ToType = (methodBase is MethodInfo methodInfo ? methodInfo.ReturnType : methodBase.DeclaringType ?? typeof(object));
			}

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

			cmp = other.IsSafeConversion.CompareTo(this.IsSafeConversion); // non-trying conversion = better
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

		internal static ConversionMethodInfo FromNativeConversion(Delegate conversionFn, ConversionQuality? conversionQualityOverride = null)
		{
			if (conversionFn == null) throw new ArgumentNullException(nameof(conversionFn));

			var methodInfo = conversionFn.GetMethodInfo()!;
			var parameters = methodInfo.GetParameters();
			var parameterTypes = new[] { ConversionParameterType.Value, ConversionParameterType.Format, ConversionParameterType.FormatProvider };

			if (parameters.Length != 3 ||
				parameters[1].ParameterType != typeof(string) ||
				parameters[2].ParameterType != typeof(IFormatProvider))
			{
				throw new InvalidOperationException("Invalid native conversion method's signature. Should be fn(value, format, formatProvider). Shouldn't be happening. Report this case to library developer.");
			}

			return new ConversionMethodInfo(methodInfo, parameters, parameterTypes, conversionQualityOverride ?? ConversionQuality.Native);
		}

		/// <inheritdoc />
		public override string ToString() => $"From: {this.FromType.Name}, To: {this.ToType.Name}, Method: {this.Method.Name} ({string.Join(", ", this.Parameters.Select(p => p.Name))}), Quality: {this.Quality}";

	}
}