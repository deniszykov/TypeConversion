using System;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Describes how conversion from <see cref="FromType"/> to <see cref="ToType"/> will be performed.
	/// </summary>
	[PublicAPI]
	public class ConversionDescriptor
	{
		/// <summary>
		/// List of conversion methods. From most preferred to least preferred. Collection not empty.
		/// </summary>
		public readonly ReadOnlyCollection<ConversionMethodInfo> Methods;
		/// <summary>
		/// Default format used for conversion. Usage of this parameter by <see cref="IConverter{FromType,ToType}"/> depends on <see cref="ConversionOptions"/>.
		/// </summary>
		public readonly string? DefaultFormat;
		/// <summary>
		/// Default format provider used for conversion. Usage of this parameter by <see cref="IConverter{FromType,ToType}"/> depends on <see cref="ConversionOptions"/>.
		/// </summary>
		public readonly IFormatProvider DefaultFormatProvider;
		/// <summary>
		/// Conversion function.
		/// </summary>
		public readonly Delegate Conversion; // Func<FromType, string, IFormatProvider, ToType> or Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>
		/// <summary>
		/// Safe conversion function. If null then <see cref="Conversion"/> function is used inside try/catch block.
		/// </summary>
		public readonly Delegate? SafeConversion; // Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>

		/// <summary>
		/// Conversion source type.
		/// </summary>
		public Type FromType => this.Methods[0].FromType;
		/// <summary>
		/// Conversion destination type.
		/// </summary>
		public Type ToType => this.Methods[0].ToType;

		/// <summary>
		/// Returns true if any conversion method exists between <see cref="FromType"/> and <see cref="ToType"/> types.
		/// </summary>
		public bool HasSomeConversion => this.Methods.Count > 1 || this.Methods[0].Quality != ConversionQuality.None;

		/// <summary>
		/// Constructor for <see cref="ConversionDescriptor"/>.
		/// </summary>
		/// <param name="methods">One or more methods. Value for <see cref="Methods"/>.</param>
		/// <param name="defaultFormat">Value for <see cref="DefaultFormat"/>.</param>
		/// <param name="defaultFormatProvider">Value for <see cref="DefaultFormatProvider"/>. If value is null then <see cref="CultureInfo.InvariantCulture"/> is used.</param>
		/// <param name="conversion">Value for <see cref="Conversion"/>.</param>
		/// <param name="safeConversion">Value for <see cref="SafeConversion"/>.</param>
		public ConversionDescriptor(
			 ReadOnlyCollection<ConversionMethodInfo> methods,
			 string? defaultFormat,
			 IFormatProvider? defaultFormatProvider,
			 Delegate conversion,
			 Delegate? safeConversion)
		{
			if (methods == null) throw new ArgumentNullException(nameof(methods));
			if (conversion == null) throw new ArgumentNullException(nameof(conversion));
			if (methods.Count == 0) throw new ArgumentOutOfRangeException(nameof(methods));

			CheckConversionDelegate(methods[0], conversion);

			if (safeConversion != null)
			{
				CheckSafeConversionDelegate(methods[0], safeConversion);
			}

			this.Methods = methods;
			this.DefaultFormat = defaultFormat;
			this.DefaultFormatProvider = defaultFormatProvider ?? CultureInfo.InvariantCulture;
			this.Conversion = conversion;
			this.SafeConversion = safeConversion;
		}

		private static void CheckConversionDelegate(ConversionMethodInfo method, Delegate conversion)
		{
			var conversionDelegateType = conversion.GetType();
			if (conversionDelegateType.GetTypeInfo().IsGenericType == false || conversionDelegateType.GetTypeInfo().GetGenericTypeDefinition() != typeof(Func<,,,>))
			{
				throw new ArgumentException($"Invalid conversion delegate type '{conversionDelegateType.FullName}'. " +
					$"An instantiation of '{typeof(Func<,,,>).FullName}' is expected.");
			}

			var expectedConversionGenericArguments = new[] { method.FromType, typeof(string), typeof(IFormatProvider), method.ToType };
			if (conversionDelegateType.GetTypeInfo().GetGenericArguments().Take(3).SequenceEqual(expectedConversionGenericArguments.Take(3)) == false)
			{
				throw new ArgumentException($"Invalid conversion delegate type '{conversionDelegateType.FullName}'. " +
					$"An instance of '{typeof(Func<,,,>).FullName}' with `{string.Join(", ", expectedConversionGenericArguments.Select(t => t.FullName))}` generic parameters  is expected.", nameof(conversion));
			}
		}
		private static void CheckSafeConversionDelegate(ConversionMethodInfo method, Delegate safeConversion)
		{
			var safeConversionDelegateType = safeConversion.GetType();
			var expectedSafeConversionGenericArguments = new[] { method.FromType, typeof(string), typeof(IFormatProvider), method.ToType };
			if (safeConversionDelegateType.GetTypeInfo().IsGenericType == false ||
				safeConversionDelegateType.GetTypeInfo().GetGenericTypeDefinition() != typeof(Func<,,,>))
			{
				throw new ArgumentException($"Invalid safe conversion delegate type '{safeConversionDelegateType.FullName}'. " +
					$"An instantiation of '{typeof(Func<,,,>).FullName}' is expected.");
			}

			if (safeConversionDelegateType.GetTypeInfo().GetGenericArguments().Take(3).SequenceEqual(expectedSafeConversionGenericArguments.Take(3)) == false)
			{
				throw new ArgumentException($"Invalid safe conversion delegate type '{safeConversionDelegateType.FullName}'. " +
					$"An instance of '{typeof(Func<,,,>).FullName}' with `{string.Join(", ", expectedSafeConversionGenericArguments.Select(t => t.FullName))}` generic parameters  is expected.", nameof(safeConversion));
			}
		}

		/// <inheritdoc />
		public override string ToString() => $"From: {this.FromType.Name}, To: {this.ToType.Name}, Preferred Method: ({this.Methods[0]}), Default Format: {this.DefaultFormat}, Default Format Provider: {this.DefaultFormatProvider}";
	}

}