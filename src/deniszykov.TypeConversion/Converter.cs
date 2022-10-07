using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

#if NETCOREAPP3_0 || NETSTANDARD2_1
using AllowNull = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
using MaybeNull = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
#else
using AllowNull = JetBrains.Annotations.CanBeNullAttribute;
using MaybeNull = JetBrains.Annotations.CanBeNullAttribute;
// ReSharper disable AnnotationRedundancyInHierarchy
#endif

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Provides type conversion methods from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/>.
	/// </summary>
	[PublicAPI]
	public sealed class Converter<FromTypeT, ToTypeT> : IConverter<FromTypeT, ToTypeT>
	{
		private readonly ITypeConversionProvider typeConversionProvider;
		private readonly ConversionOptions converterOptions;

		/// <inheritdoc />
		public ConversionDescriptor Descriptor { get; }
		/// <inheritdoc />
		Type IConverter.FromType => typeof(FromTypeT);
		/// <inheritdoc />
		Type IConverter.ToType => typeof(ToTypeT);
		/// <inheritdoc />
		void IConverter.Convert(object? value, out object? result, string? format, IFormatProvider? formatProvider)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToTypeT)
			{
				result = value;
				return;
			}

			this.Convert((FromTypeT)value!, out var resultTyped, format, formatProvider);
			result = resultTyped;
		}
		/// <inheritdoc />
		bool IConverter.TryConvert(object? value, out object? result, string? format, IFormatProvider? formatProvider)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToTypeT)
			{
				result = value;
				return true;
			}

			var success = this.TryConvert((FromTypeT)value!, out var resultTyped, format, formatProvider);
			result = resultTyped;
			return success;
		}

		public Converter(ITypeConversionProvider typeConversionProvider, ConversionDescriptor conversion, ConversionOptions converterOptions)
		{
			if (typeConversionProvider == null) throw new ArgumentNullException(nameof(typeConversionProvider));
			if (conversion == null) throw new ArgumentNullException(nameof(conversion));

			this.Descriptor = conversion;
			this.typeConversionProvider = typeConversionProvider;
			this.converterOptions = converterOptions;
			this.converterOptions = converterOptions;
		}

		/// <inheritdoc />
		public void Convert([AllowNull] FromTypeT value, [MaybeNull] out ToTypeT result, string? format = null, IFormatProvider? formatProvider = null)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatIfNotSpecified) && format == null)
			{
				format = this.Descriptor.DefaultFormat;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatProviderIfNotSpecified) && formatProvider == null)
			{
				formatProvider = this.Descriptor.DefaultFormatProvider;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToTypeT typedValue)
			{
				result = typedValue;
				return;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.PromoteValueToActualType) &&
				ReferenceEquals(value, null) == false &&
				value.GetType() != typeof(FromTypeT))
			{
				var actualConverter = this.typeConversionProvider.GetConverter(value.GetType(), typeof(ToTypeT));
				if (actualConverter.Descriptor.HasSomeConversion)
				{
					actualConverter.Convert(value, out var resultObj, format, formatProvider);
					result = (ToTypeT)resultObj;
					return;
				}
			}

			var convertFn = (Func<FromTypeT, string?, IFormatProvider?, ToTypeT>)this.Descriptor.Conversion;
			result = convertFn(value!, format, formatProvider);
		}
		/// <inheritdoc />
		public bool TryConvert([AllowNull] FromTypeT value, [MaybeNull] out ToTypeT result, string? format = null, IFormatProvider? formatProvider = null)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatIfNotSpecified) && format == null)
			{
				format = this.Descriptor.DefaultFormat;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatProviderIfNotSpecified) && formatProvider == null)
			{
				formatProvider = this.Descriptor.DefaultFormatProvider;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToTypeT typedValue)
			{
				result = typedValue;
				return true;
			}

			if (this.converterOptions.HasFlag(ConversionOptions.PromoteValueToActualType) &&
				ReferenceEquals(value, null) == false &&
				value.GetType() != typeof(FromTypeT))
			{
				var actualConverter = this.typeConversionProvider.GetConverter(value.GetType(), typeof(ToTypeT));
				if (actualConverter.Descriptor.HasSomeConversion)
				{
					var converted = actualConverter.TryConvert(value, out var resultObj, format, formatProvider);
					result = (ToTypeT)resultObj;
					return converted;
				}
			}


			var safeConvertFn = (Func<FromTypeT, string?, IFormatProvider?, KeyValuePair<ToTypeT, bool>>?)this.Descriptor.SafeConversion;
			if (safeConvertFn != null)
			{
				var resultOrFail = safeConvertFn(value!, format, formatProvider);
				result = resultOrFail.Key;
				return resultOrFail.Value;
			}
			else
			{
				result = default!;
				try
				{
					this.Convert(value, out result, format, formatProvider);
					return true;
				}
				catch (Exception e)
				{
					if (e is InvalidCastException || e is FormatException ||
						e is ArithmeticException || e is NotSupportedException ||
						e is ArgumentException || e is InvalidTimeZoneException) // TODO make exception list configurable
						return false;
					throw;
				}
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return this.Descriptor.ToString();
		}
	}
}
