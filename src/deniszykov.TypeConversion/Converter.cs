using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public sealed class Converter<FromType, ToType> : IConverter<FromType, ToType>
	{
		/// <inheritdoc />
		public ConversionInfo Info { get; }
		/// <inheritdoc />
		Type IConverter.FromType => typeof(FromType);
		/// <inheritdoc />
		Type IConverter.ToType => typeof(ToType);
		/// <inheritdoc />
		void IConverter.Convert(object value, out object result, string format, IFormatProvider formatProvider)
		{
			this.Convert((FromType)value, out var resultTyped, format, formatProvider);
			result = resultTyped;
		}
		/// <inheritdoc />
		bool IConverter.TryConvert(object value, out object result, string format, IFormatProvider formatProvider)
		{
			var success = this.TryConvert((FromType)value, out var resultTyped, format, formatProvider);
			result = resultTyped;
			return success;
		}

		public Converter([NotNull]ConversionInfo conversion)
		{
			if (conversion == null) throw new ArgumentNullException(nameof(conversion));

			this.Info = conversion;
		}

		/// <inheritdoc />
		public void Convert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null)
		{
			var convertFn = (Func<FromType, string, IFormatProvider, ToType>)this.Info.Conversion;
			result = convertFn(value, format ?? this.Info.DefaultFormat, formatProvider);
		}
		/// <inheritdoc />
		public bool TryConvert(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null)
		{
			var safeConvertFn = (Func<FromType, string, IFormatProvider, KeyValuePair<ToType, bool>>)this.Info.SafeConversion;
			if (safeConvertFn != null)
			{
				var resultOrFail = safeConvertFn(value, format ?? this.Info.DefaultFormat, formatProvider);
				result = resultOrFail.Key;
				return resultOrFail.Value;
			}
			else
			{
				result = default;
				try
				{
					this.Convert(value, out result, format ?? this.Info.DefaultFormat, formatProvider);
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
