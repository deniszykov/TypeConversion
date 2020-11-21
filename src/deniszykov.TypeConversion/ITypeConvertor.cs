using System;
#if !NETSTANDARD
using System.ComponentModel;
#endif
using System.Reflection;

namespace TypeConvert
{
	public interface ITypeConvertor
	{
		ConversionInfo GetInfo<FromType, ToType>();
		ConversionInfo GetInfo(Type fromType, Type toType);

		void Convert<FromType, ToType>(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);
		bool TryConvert<FromType, ToType>(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null);

		void Convert(Type toType, object value, out object result, string format = null, IFormatProvider formatProvider = null);
		bool TryConvert(Type toType, object value, out object result, string format = null, IFormatProvider formatProvider = null);
	}


	public class ConversionInfo
	{
		public readonly ConversionMethod Method;
		public readonly MethodInfo MethodOrOperator;
		public readonly ConstructorInfo Constructor;
		public readonly Delegate CustomConversion;
#if !NETSTANDARD
		public readonly TypeConverter TypeConverter;
#endif
		public readonly Delegate Conversion; // Func<FromT, string, IFormatProvider, ToType>

	}

	public enum ConversionMethod
	{
		/// <summary>
		/// Constructor accepting appropriate type.
		/// </summary>
		Constructor,
		/// <summary>
		/// <see cref="System.ComponentModel.TypeConverter"/> instance.
		/// </summary>
		TypeConverter,
		/// <summary>
		/// Conversion method on type (Parse, ToXXX, FromXXX)
		/// </summary>
		Method,
		/// <summary>
		/// Explicit conversion operator.
		/// </summary>
		Explicit,
		/// <summary>
		/// Implicit conversion operator.
		/// </summary>
		Implicit,
		/// <summary>
		/// Runtime provided conversion method for build-in types.
		/// </summary>
		Native,
		/// <summary>
		/// User-provided conversion function.
		/// </summary>
		Custom
	}
}
