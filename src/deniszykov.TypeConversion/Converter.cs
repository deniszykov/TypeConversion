using System;
using System.Collections.Generic;
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
	/// Provides type conversion methods from <typeparamref name="FromType"/> to <typeparamref name="ToType"/>.
	/// </summary>
	[PublicAPI]
	public sealed class Converter<FromType, ToType> : IConverter<FromType, ToType>
	{
		private readonly ConversionOptions converterOptions;

		/// <inheritdoc />
		public ConversionDescriptor Descriptor { get; }
		/// <inheritdoc />
		Type IConverter.FromType => typeof(FromType);
		/// <inheritdoc />
		Type IConverter.ToType => typeof(ToType);
		/// <inheritdoc />
		void IConverter.Convert(object? value, out object? result, string? format, IFormatProvider? formatProvider)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToType)
			{
				result = value;
				return;
			}

			this.Convert((FromType)value!, out var resultTyped, format, formatProvider);
			result = resultTyped;
		}
		/// <inheritdoc />
		bool IConverter.TryConvert(object? value, out object? result, string? format, IFormatProvider? formatProvider)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToType)
			{
				result = value;
				return true;
			}

			var success = this.TryConvert((FromType)value!, out var resultTyped, format, formatProvider);
			result = resultTyped;
			return success;
		}

		public Converter(ConversionDescriptor conversion, ConversionOptions converterOptions)
		{
			if (conversion == null) throw new ArgumentNullException(nameof(conversion));

			this.Descriptor = conversion;
			this.converterOptions = converterOptions;
			this.converterOptions = converterOptions;
		}

		/// <inheritdoc />
		public void Convert([AllowNull] FromType value, [MaybeNull] out ToType result, string? format = null, IFormatProvider? formatProvider = null)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatIfNotSpecified) && format == null)
			{
				format = this.Descriptor.DefaultFormat;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatProviderIfNotSpecified) && formatProvider == null)
			{
				formatProvider = this.Descriptor.DefaultFormatProvider;
			}

			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToType typedValue)
			{
				result = typedValue;
				return;
			}

			var convertFn = (Func<FromType, string?, IFormatProvider?, ToType>)this.Descriptor.Conversion;
			result = convertFn(value!, format, formatProvider);
		}
		/// <inheritdoc />
		public bool TryConvert([AllowNull] FromType value, [MaybeNull] out ToType result, string? format = null, IFormatProvider? formatProvider = null)
		{
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatIfNotSpecified) && format == null)
			{
				format = this.Descriptor.DefaultFormat;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.UseDefaultFormatProviderIfNotSpecified) && formatProvider == null)
			{
				formatProvider = this.Descriptor.DefaultFormatProvider;
			}
			if (this.converterOptions.HasFlag(ConversionOptions.FastCast) && value is ToType typedValue)
			{
				result = typedValue;
				return true;
			}

			var safeConvertFn = (Func<FromType, string?, IFormatProvider?, KeyValuePair<ToType, bool>>?)this.Descriptor.SafeConversion;
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
	}
}
