using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public class ConversionDescriptor
	{
		[NotNull]
		public readonly ReadOnlyCollection<ConversionMethodInfo> Methods;
		[CanBeNull]
		public readonly string DefaultFormat;
		[NotNull]
		public readonly IFormatProvider DefaultFormatProvider;
		[NotNull]
		public readonly Delegate Conversion; // Func<FromType, string, IFormatProvider, ToType>
		[CanBeNull]
		public readonly Delegate SafeConversion; // Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>

		[NotNull]
		public Type FromType => this.Methods[0].FromType;
		[NotNull]
		public Type ToType => this.Methods[0].ToType;

		public ConversionDescriptor([NotNull, ItemNotNull] ReadOnlyCollection<ConversionMethodInfo> methods, [CanBeNull] string defaultFormat, IFormatProvider defaultFormatProvider, [NotNull] Delegate conversion, [CanBeNull] Delegate safeConversion)
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
			this.DefaultFormatProvider = defaultFormatProvider;
			this.Conversion = conversion;
			this.SafeConversion = safeConversion;
		}

		private static void CheckConversionDelegate(ConversionMethodInfo method, Delegate conversion)
		{
			var conversionDelegateType = conversion.GetType();
			var expectedConversionGenericArguments = new[] { method.FromType, typeof(string), typeof(IFormatProvider), method.ToType };
			if (conversionDelegateType.GetTypeInfo().IsGenericType == false || conversionDelegateType.GetTypeInfo().GetGenericTypeDefinition() != typeof(Func<,,,>))
				throw new ArgumentException($"Invalid conversion delegate type '{conversionDelegateType.FullName}'. An instantiation of '{typeof(Func<,,,>).FullName}' is expected.");
			if (conversionDelegateType.GetTypeInfo().GetGenericArguments().SequenceEqual(expectedConversionGenericArguments) == false)
				throw new ArgumentException(
					$"Invalid conversion delegate type '{conversionDelegateType.FullName}'. An instance of '{typeof(Func<,,,>).FullName}' with `{string.Join(", ", expectedConversionGenericArguments.Select(t => t.FullName))}` generic parameters  is expected.", nameof(conversion));
		}
		private static void CheckSafeConversionDelegate(ConversionMethodInfo method, Delegate safeConversion)
		{
			var safeConversionDelegateType = safeConversion.GetType();
			var expectedSafeConversionGenericArguments = new[] { method.FromType, typeof(string), typeof(IFormatProvider) };
			if (safeConversionDelegateType.GetTypeInfo().IsGenericType == false || safeConversionDelegateType.GetTypeInfo().GetGenericTypeDefinition() != typeof(Func<,,,>))
				throw new ArgumentException($"Invalid safe conversion delegate type '{safeConversionDelegateType.FullName}'. An instantiation of '{typeof(Func<,,,>).FullName}' is expected.");
			if (safeConversionDelegateType.GetTypeInfo().GetGenericArguments().Take(3).SequenceEqual(expectedSafeConversionGenericArguments) == false)
				throw new ArgumentException($"Invalid safe conversion delegate type '{safeConversionDelegateType.FullName}'. An instance of '{typeof(Func<,,,>).FullName}' with `{string.Join(", ", expectedSafeConversionGenericArguments.Select(t => t.FullName))}` generic parameters  is expected.", nameof(safeConversion));
		}

		/// <inheritdoc />
		public override string ToString() => $"Preferred Method: ({this.Methods[0]}), Default Format: {this.DefaultFormat}, Default Format Provider: {this.DefaultFormatProvider}";
	}

}