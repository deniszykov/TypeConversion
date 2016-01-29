/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ConvertUntypedDelegate = System.Func<object, string, System.IFormatProvider, object>;

// ReSharper disable once CheckNamespace
namespace System
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	public static class TypeConvert
	{
		[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
		private static class BetweenTypes<FromType, ToType>
		{
			public static readonly TypeConverter Converter;
			public static readonly Func<FromType, ToType> ExplicitFrom;
			public static readonly Func<FromType, ToType> ImplicitFrom;
			public static readonly Func<ToType, FromType> ExplicitTo;
			public static readonly Func<ToType, FromType> ImplicitTo;
			public static readonly Func<ToType, FromType> ConvertibleTo;
			public static readonly Func<FromType, ToType> ConvertibleFrom;
			public static readonly Func<FromType, string, IFormatProvider, ToType> Transition;

			static BetweenTypes()
			{
				const BindingFlags methodVisibility = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

				var sourceType = typeof(FromType);
				var resultType = typeof(ToType);

				var isResultNullableValueType = (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Nullable<>));
				var isSourceNullableValueType = (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>));
				var isSourceIsObject = sourceType == typeof(Object);
				var isResultIsObject = resultType == typeof(Object);
				var isSourceIsEnum = sourceType.IsEnum;
				var isResultIsEnum = resultType.IsEnum;
				var isSourceIsString = sourceType == typeof(String);
				var isResultIsString = resultType == typeof(String);

				var transitionMethod = default(MethodInfo);

				if (isResultNullableValueType && isSourceNullableValueType)
					transitionMethod = typeof(TypeConvert).GetMethod("ConvertFromNullableToNullable", methodVisibility).MakeGenericMethod(Nullable.GetUnderlyingType(resultType), Nullable.GetUnderlyingType(sourceType));
				else if (isResultNullableValueType)
					transitionMethod = typeof(TypeConvert).GetMethod("ConvertToNullable", methodVisibility).MakeGenericMethod(Nullable.GetUnderlyingType(resultType), sourceType);
				else if (isSourceNullableValueType)
					transitionMethod = typeof(TypeConvert).GetMethod("ConvertFromNullable", methodVisibility).MakeGenericMethod(resultType, Nullable.GetUnderlyingType(sourceType));
				else if (isResultIsObject)
					transitionMethod = typeof(TypeConvert).GetMethod("ConvertToObject", methodVisibility).MakeGenericMethod(resultType, sourceType);
				else if (isSourceIsObject)
					transitionMethod = typeof(TypeConvert).GetMethod("ConvertFromObject", methodVisibility).MakeGenericMethod(resultType, sourceType);
				else if (isSourceIsEnum && isResultIsEnum)
				{
					transitionMethod = EnumInterchange ?
						ConvertMethodInfo.MakeGenericMethod(Enum.GetUnderlyingType(sourceType), Enum.GetUnderlyingType(resultType)) :
						typeof(TypeConvert).GetMethod("BetweenEnums", methodVisibility).MakeGenericMethod(resultType, Enum.GetUnderlyingType(resultType), sourceType, Enum.GetUnderlyingType(sourceType));
				}
				else if (isSourceIsEnum && !isResultIsString)
				{
					transitionMethod = EnumInterchange ?
						ConvertMethodInfo.MakeGenericMethod(Enum.GetUnderlyingType(sourceType), resultType) :
						typeof(TypeConvert).GetMethod("FromEnum", methodVisibility).MakeGenericMethod(resultType, sourceType, Enum.GetUnderlyingType(sourceType));
				}
				else if (isResultIsEnum && !isSourceIsString)
				{
					transitionMethod = EnumInterchange ?
						ConvertMethodInfo.MakeGenericMethod(sourceType, Enum.GetUnderlyingType(resultType)) :
						typeof(TypeConvert).GetMethod("ToEnum", methodVisibility).MakeGenericMethod(resultType, Enum.GetUnderlyingType(resultType), sourceType);
				}

				if (transitionMethod != null)
				{
					Transition = (Func<FromType, string, IFormatProvider, ToType>)Delegate.CreateDelegate(typeof(Func<FromType, string, IFormatProvider, ToType>), transitionMethod);
					return;
				}

				foreach (var method in resultType.GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					if (method.IsSpecialName && method.Name == "op_Explicit" && method.ReturnType == sourceType && HasSingleParameterOfType(method, resultType))
						ExplicitTo = (Func<ToType, FromType>)Delegate.CreateDelegate(typeof(Func<ToType, FromType>), method, true);
					else if (method.IsStatic && method.IsSpecialName && method.Name == "op_Implicit" && method.ReturnType == sourceType && HasSingleParameterOfType(method, resultType))
						ImplicitTo = (Func<ToType, FromType>)Delegate.CreateDelegate(typeof(Func<ToType, FromType>), method, true);
					else if (method.IsSpecialName && method.Name == "op_Explicit" && method.ReturnType == resultType && HasSingleParameterOfType(method, sourceType))
						ExplicitFrom = (Func<FromType, ToType>)Delegate.CreateDelegate(typeof(Func<FromType, ToType>), method, true);
					else if (method.IsSpecialName && method.Name == "op_Implicit" && method.ReturnType == resultType && HasSingleParameterOfType(method, sourceType))
						ImplicitFrom = (Func<FromType, ToType>)Delegate.CreateDelegate(typeof(Func<FromType, ToType>), method, true);
				}
				foreach (var method in typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					if (method.Name.StartsWith("To", StringComparison.Ordinal) && method.ReturnType == sourceType && HasSingleParameterOfType(method, resultType))
						ConvertibleTo = (Func<ToType, FromType>)Delegate.CreateDelegate(typeof(Func<ToType, FromType>), method, true);
					else if (method.Name.StartsWith("To", StringComparison.Ordinal) && method.ReturnType == resultType && HasSingleParameterOfType(method, sourceType))
						ConvertibleFrom = (Func<FromType, ToType>)Delegate.CreateDelegate(typeof(Func<FromType, ToType>), method, true);
				}

				Converter = TypeDescriptor.GetConverter(sourceType);
				if (Converter != null && Converter.GetType() == typeof(TypeConverter))
					Converter = null;
			}

			private static bool HasSingleParameterOfType(MethodInfo method, Type type)
			{
				var @params = method.GetParameters();
				return @params.Length == 1 && @params[0].ParameterType == type;
			}
		}

		[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
		private static class TypeInfo<ToType>
		{
			// nullable
			public static readonly bool IsNullableValueType;
			public static readonly bool IsString;
			public static readonly bool IsValueType;
			public static readonly bool IsFormattable;

			public static readonly TypeConverter Converter;
			public static readonly Func<string, ToType> ParseFn;
			public static readonly Func<string, IFormatProvider, ToType> ParseFormatedFn;
			public static readonly Func<ToType, string> ToStringFn;
			public static readonly Func<ToType, string, IFormatProvider, string> ToStringFormatedFn;
			public static readonly string DefaultFormat;

			static TypeInfo()
			{
				var resultT = typeof(ToType);

				IsValueType = resultT.IsValueType;

				if (resultT.IsGenericType && resultT.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					IsNullableValueType = true;
					return;
				}

				if (resultT.IsEnum)
				{
					Converter = new EnumConverter(resultT);
					return;
				}

				IsString = typeof(string) == resultT;
				IsFormattable = typeof(IFormattable).IsAssignableFrom(resultT);

				Converter = TypeDescriptor.GetConverter(typeof(ToType));
				if (Converter != null && Converter.GetType() == typeof(TypeConverter))
					Converter = null;

				if (typeof(ToType) == typeof(Single) || typeof(ToType) == typeof(Double))
					DefaultFormat = "R";

				foreach (var method in typeof(ToType).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					if (method.IsStatic && (method.Name == "Parse" || method.Name == "Create") && method.ReturnType == typeof(ToType))
					{
						var parseParams = method.GetParameters();
						if (parseParams.Length == 1 && parseParams[0].ParameterType == typeof(string))
							ParseFn = (Func<string, ToType>)Delegate.CreateDelegate(typeof(Func<string, ToType>), method, true);
						else if (parseParams.Length == 2 && parseParams[0].ParameterType == typeof(string) && parseParams[1].ParameterType == typeof(IFormatProvider))
							ParseFormatedFn = (Func<string, IFormatProvider, ToType>)Delegate.CreateDelegate(typeof(Func<string, IFormatProvider, ToType>), method, true);
					}
					else if (!method.IsStatic && method.Name == "ToString" && method.ReturnType == typeof(String))
					{
						var toStringParams = method.GetParameters();
						if (toStringParams.Length == 0)
							ToStringFn = (Func<ToType, string>)Delegate.CreateDelegate(typeof(Func<ToType, string>), null, method, false);
						else if (toStringParams.Length == 2 && toStringParams[0].ParameterType == typeof(string) && toStringParams[1].ParameterType == typeof(IFormatProvider))
							ToStringFormatedFn = (Func<ToType, string, IFormatProvider, string>)Delegate.CreateDelegate(typeof(Func<ToType, string, IFormatProvider, string>), null, method, false);
					}
				}
			}
		}

		private static readonly Dictionary<long, ConvertUntypedDelegate> CachedGenericConvertMethods = new Dictionary<long, ConvertUntypedDelegate>();
		private static readonly MethodInfo ConvertMethodInfo = typeof(TypeConvert).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == "Convert" && m.IsGenericMethod);
		private static readonly bool EnumInterchange = TestEnumInterchange();
		public static readonly IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;

		public static ToType Convert<FromType, ToType>(FromType value, string format = null, IFormatProvider formatProvider = null)
		{
			return BetweenTypes<FromType, ToType>.Transition != null ?
				BetweenTypes<FromType, ToType>.Transition(value, format, formatProvider) :
				InternalConvert<FromType, ToType>(value, format, formatProvider);
		}
		public static bool TryConvert<FromType, ToType>(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null)
		{
			result = default(ToType);
			try
			{
				result = Convert<FromType, ToType>(value, format, formatProvider);
				return true;
			}
			catch (Exception e)
			{
				if (e is InvalidCastException || e is FormatException ||
					e is ArithmeticException || e is NotSupportedException ||
					e is ArgumentException || e is InvalidTimeZoneException)
					return false;
				throw;
			}
		}
		public static string ToString<FromType>(FromType value, string format = null, IFormatProvider formatProvider = null)
		{
			return Convert<FromType, string>(value, format, formatProvider);
		}

		public static object Convert(Type fromType, Type toType, object value, string format = null, IFormatProvider formatProvider = null)
		{
			if (toType == null) throw new ArgumentNullException("toType");
			if (fromType == null) throw new ArgumentNullException("fromType");

			var convertFn = default(ConvertUntypedDelegate);
			var fromHash = fromType.GetHashCode(); // it's not hashcode, it's an unique sync-lock of type-object
			var toHash = toType.GetHashCode();
			var cacheKey = unchecked(((long)fromHash << 32) | (uint)toHash);

			var gotFromCache = false;
			lock (CachedGenericConvertMethods)
				gotFromCache = CachedGenericConvertMethods.TryGetValue(cacheKey, out convertFn);

			if (gotFromCache)
				return convertFn.Invoke(value, format, formatProvider);

			var convMethod = ConvertMethodInfo.MakeGenericMethod(fromType, toType);
			var valueParam = Expression.Parameter(typeof(object), "value");
			var formatParam = Expression.Parameter(typeof(string), "format");
			var formatProviderParam = Expression.Parameter(typeof(IFormatProvider), "formatProvider");
			var convExpression = Expression.Lambda<ConvertUntypedDelegate>
			(
				Expression.Convert
					(
						Expression.Call
							(
								convMethod,
								Expression.Convert(valueParam, fromType),
								formatParam,
								formatProviderParam
							),
						typeof(object)
					),
				valueParam,
				formatParam,
				formatProviderParam
			);

			lock (CachedGenericConvertMethods)
				CachedGenericConvertMethods[cacheKey] = convertFn = convExpression.Compile();

			return convertFn.Invoke(value, format, formatProvider);
		}
		public static bool TryConvert(Type fromType, Type toType, ref object value, string format = null, IFormatProvider formatProvider = null)
		{
			try
			{
				value = Convert(fromType, toType, value, format, formatProvider);
				return true;
			}
			catch (Exception e)
			{
				if (e is InvalidCastException || e is FormatException ||
					e is ArithmeticException || e is NotSupportedException ||
					e is ArgumentException || e is InvalidTimeZoneException)
					return false;
				throw;
			}
		}
		public static string ToString(object value, string format = null, IFormatProvider formatProvider = null)
		{
			if (value == null || value is string)
				return (string)value;

			return (string)Convert(typeof(object), typeof(string), value, format, formatProvider);
		}

		private static ToType InternalConvert<FromType, ToType>(FromType value, string format, IFormatProvider formatProvider)
		{
			if (formatProvider == null) formatProvider = DefaultFormatProvider;

			var sourceObj = default(object);
			var sourceIsNullRef = default(bool);
			var sourceIsValueType = TypeInfo<FromType>.IsValueType;

			if (sourceIsValueType)
			{
				sourceIsNullRef = false;
				if (typeof(FromType) == typeof(ToType))
				{
#if NO_TYPE_REFS
					return (ToType)sourceObj;
#else
					return __refvalue(__makeref(value), ToType);
#endif
				}
			}
			else
			{
				sourceObj = value; // (1) box ref-type
				sourceIsNullRef = sourceObj == null;

				if (sourceObj is ToType)
					return (ToType)sourceObj;
			}

			// find explicit/implicit convertions between types
			if (BetweenTypes<FromType, ToType>.ImplicitFrom != null)
				return BetweenTypes<FromType, ToType>.ImplicitFrom(value);
			if (BetweenTypes<ToType, FromType>.ImplicitTo != null)
				return BetweenTypes<ToType, FromType>.ImplicitTo(value);
			if (BetweenTypes<FromType, ToType>.ExplicitFrom != null)
				return BetweenTypes<FromType, ToType>.ExplicitFrom(value);
			if (BetweenTypes<ToType, FromType>.ExplicitTo != null)
				return BetweenTypes<ToType, FromType>.ExplicitTo(value);
			// try parse
			if (TypeInfo<FromType>.IsString && TypeInfo<ToType>.ParseFormatedFn != null)
				return TypeInfo<ToType>.ParseFormatedFn((string)sourceObj, formatProvider);
			if (TypeInfo<FromType>.IsString && TypeInfo<ToType>.ParseFn != null)
				return TypeInfo<ToType>.ParseFn((string)sourceObj);
			// try toString(formatted)
			if (!sourceIsNullRef && TypeInfo<ToType>.IsString && TypeInfo<FromType>.ToStringFormatedFn != null)
				return (ToType)(object)TypeInfo<FromType>.ToStringFormatedFn(value, format ?? TypeInfo<FromType>.DefaultFormat, formatProvider);
			if (!sourceIsNullRef && TypeInfo<ToType>.IsString && TypeInfo<FromType>.IsFormattable)
				// ReSharper disable once PossibleNullReferenceException
				return (ToType)(object)((sourceIsValueType ? (object)value : sourceObj) as IFormattable).ToString(format ?? TypeInfo<FromType>.DefaultFormat, formatProvider); // (2) box value-type
																																											   // try convertible
			if (BetweenTypes<ToType, FromType>.ConvertibleTo != null)
				return BetweenTypes<ToType, FromType>.ConvertibleTo(value);
			if (BetweenTypes<FromType, ToType>.ConvertibleFrom != null)
				return BetweenTypes<FromType, ToType>.ConvertibleFrom(value);
			// try converter			
			if (TypeInfo<ToType>.Converter != null && TypeInfo<ToType>.Converter.CanConvertFrom(typeof(FromType)))
				return (ToType)TypeInfo<ToType>.Converter.ConvertFrom(null, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, (sourceIsValueType ? value : sourceObj)); // (3) box value-type
			if (BetweenTypes<FromType, ToType>.Converter != null && BetweenTypes<FromType, ToType>.Converter.CanConvertTo(typeof(ToType)))
				return (ToType)BetweenTypes<FromType, ToType>.Converter.ConvertTo(null, formatProvider as CultureInfo, (sourceIsValueType ? value : sourceObj), typeof(ToType)); // (4) box value-type
																																												 // try ToString
			if (!sourceIsNullRef && TypeInfo<ToType>.IsString && TypeInfo<FromType>.ToStringFn != null)
				return (ToType)(object)TypeInfo<FromType>.ToStringFn(value);
			if (!sourceIsNullRef && TypeInfo<ToType>.IsString)
				return (ToType)(object)(sourceIsValueType ? (object)value : sourceObj).ToString(); // (5) box value-type
			throw new InvalidCastException(string.Format("Unable to convert value '{2}' of type '{1}' to requested type '{0}'", typeof(ToType), typeof(FromType), ((object)value) ?? "<null>")); // (6) box value-type/ref-type		
		}

		// transitions
		private static ToType? ConvertFromNullableToNullable<ToType, FromType>(FromType? value, string format, IFormatProvider formatProvider)
			where FromType : struct
			where ToType : struct
		{
			if (value == null)
				return null;

			var result = InternalConvert<FromType, ToType>(value.Value, format, formatProvider);
			return result;
		}
		private static ToType ConvertFromNullable<ToType, FromType>(FromType? value, string format, IFormatProvider formatProvider)
			where FromType : struct
		{
			var result = InternalConvert<FromType, ToType>(value.GetValueOrDefault(), format, formatProvider);
			return result;
		}
		private static ToType? ConvertToNullable<ToType, FromType>(FromType value, string format, IFormatProvider formatProvider)
			where ToType : struct
		{
			var result = InternalConvert<FromType, ToType>(value, format, formatProvider);
			return result;
		}
		private static ToType ConvertFromObject<ToType, FromType>(object value, string format, IFormatProvider formatProvider)
		{
			if (value != null && value.GetType() == typeof(object))
				throw new InvalidCastException();

			if (value == null)
			{
				var fromType = typeof(FromType);
				if (fromType.IsValueType && !TypeInfo<FromType>.IsNullableValueType)
				{
					fromType = typeof(Nullable<>).MakeGenericType(fromType);
					return (ToType)Convert(typeof(ToType), null, fromType, format, formatProvider);
				}
				if (!TypeInfo<ToType>.IsValueType)
					return default(ToType);
				throw new InvalidCastException(string.Format("Unable to cast <null> to '{0}'.", typeof(ToType)));
			}

			return (ToType)Convert(value.GetType(), typeof(ToType), value, format, formatProvider);
		}
		private static object ConvertToObject<ToType, FromType>(FromType value, string format, IFormatProvider formatProvider)
		{
			return value;
		}

		private static ToEnumT BetweenEnums<ToEnumT, ToBaseT, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
			where ToEnumT : IConvertible
			where FromEnumT : IConvertible
		{
			var result = InternalConvert<FromBaseT, ToBaseT>((FromBaseT)value.ToType(typeof(FromBaseT), formatProvider), format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), result);
		}
		private static ToEnumT ToEnum<ToEnumT, ToBaseT, FromType>(FromType value, string format, IFormatProvider formatProvider)
			where ToEnumT : IConvertible
		{
			var result = InternalConvert<FromType, ToBaseT>(value, format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), result);
		}
		private static ToType FromEnum<ToType, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
			where FromEnumT : IConvertible
		{
			var result = InternalConvert<FromBaseT, ToType>((FromBaseT)value.ToType(typeof(FromBaseT), formatProvider), format, formatProvider);
			return result;
		}

		private static bool TestEnumInterchange()
		{
			// test if platform support of enum interchange in delegates
			try
			{
				Delegate.CreateDelegate(typeof(Action<int>), typeof(Console), "set_BackgroundColor");
				Delegate.CreateDelegate(typeof(Func<ConsoleColor>), typeof(Console), "get_CursorLeft");
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
