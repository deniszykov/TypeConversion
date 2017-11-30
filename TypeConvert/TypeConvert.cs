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
// ReSharper disable StaticMemberInGenericType, UnusedVariable, InconsistentNaming
// ReSharper disable once CheckNamespace
namespace System
{
	public static class TypeConvert
	{
		private static class TypeConversion<SourceT, ResultT>
		{
#if !NETSTANDARD
			public static readonly TypeConverter Converter;
#endif
			public static readonly Func<SourceT, ResultT> ExplicitFrom;
			public static readonly Func<SourceT, ResultT> ImplicitFrom;
			public static readonly Func<ResultT, SourceT> ExplicitTo;
			public static readonly Func<ResultT, SourceT> ImplicitTo;
			public static readonly Func<ResultT, SourceT> ConvertibleTo;
			public static readonly Func<SourceT, ResultT> ConvertibleFrom;
			public static readonly Func<SourceT, string, IFormatProvider, ResultT> Transition;

			static TypeConversion()
			{

#if !NETSTANDARD
				const BindingFlags methodVisibility = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

				var sourceType = typeof(SourceT);
				var sourceTypeInfo = sourceType;
				var resultType = typeof(ResultT);
				var resultTypeInfo = resultType;
#else
				var sourceType = typeof(SourceT);
				var sourceTypeInfo = sourceType.GetTypeInfo();
				var resultType = typeof(ResultT);
				var resultTypeInfo = resultType.GetTypeInfo();
#endif
				var sourceToResultKey = GetTypePairKey(sourceType, resultType);
				var resultToSourceKey = GetTypePairKey(resultType, sourceType);

				var isSourceNullableValueType = Nullable.GetUnderlyingType(sourceType) != null;
				var isResultNullableValueType = Nullable.GetUnderlyingType(resultType) != null;
				var isSourceIsObject = sourceType == typeof(object);
				var isResultIsObject = resultType == typeof(object);
				var isSourceIsEnum = sourceTypeInfo.IsEnum;
				var isResultIsEnum = resultTypeInfo.IsEnum;
				var isSourceIsString = sourceType == typeof(string);
				var isResultIsString = resultType == typeof(string);

				var transitionMethod = default(MethodInfo);

				if (isResultIsObject)
				{
					transitionMethod = ConvertToObjectMethodDefinition.MakeGenericMethod(sourceType);
				}
				else if (isSourceIsObject)
				{
					transitionMethod = ConvertFromObjectMethodDefinition.MakeGenericMethod(resultType);
				}
				else if (isResultNullableValueType && isSourceNullableValueType)
				{
					transitionMethod = ConvertFromNullableToNullableMethodDefinition.MakeGenericMethod(Nullable.GetUnderlyingType(resultType), Nullable.GetUnderlyingType(sourceType));
				}
				else if (isResultNullableValueType)
				{
					transitionMethod = ConvertToNullableMethodDefinition.MakeGenericMethod(Nullable.GetUnderlyingType(resultType), sourceType);
				}
				else if (isSourceNullableValueType)
				{
					transitionMethod = ConvertFromNullableMethodDefinition.MakeGenericMethod(resultType, Nullable.GetUnderlyingType(sourceType));
				}
				else if (isSourceIsEnum && isResultIsEnum)
				{
					transitionMethod = ConvertFromEnumToEnumMethodDefinition.MakeGenericMethod(resultType, Enum.GetUnderlyingType(resultType), sourceType, Enum.GetUnderlyingType(sourceType));
				}
				else if (isSourceIsEnum && !isResultIsString)
				{
					transitionMethod = ConvertFromEnumMethodDefinition.MakeGenericMethod(resultType, sourceType, Enum.GetUnderlyingType(sourceType));
				}
				else if (isResultIsEnum && !isSourceIsString)
				{
					transitionMethod = ConvertToEnumMethodDefinition.MakeGenericMethod(resultType, Enum.GetUnderlyingType(resultType), sourceType);
				}

				if (transitionMethod != null)
				{
					Transition = (Func<SourceT, string, IFormatProvider, ResultT>)CreateDelegate(typeof(Func<SourceT, string, IFormatProvider, ResultT>), transitionMethod);
					return;
				}

				foreach (var method in GetPublicMethods(resultTypeInfo, declaredOnly: false))
				{
					if (method.IsStatic == false || method.Name.StartsWith("op_", StringComparison.Ordinal) == false || method.IsSpecialName == false)
						continue;

					var parameters = method.GetParameters();
					if (parameters.Length != 1)
						continue;

					var firstParameterType = parameters[0].ParameterType;
					if (method.Name == "op_Explicit" && method.ReturnType == sourceType && firstParameterType == resultType)
						ExplicitTo = (Func<ResultT, SourceT>)CreateDelegate(typeof(Func<ResultT, SourceT>), method, true);
					else if (method.Name == "op_Implicit" && method.ReturnType == sourceType && firstParameterType == resultType)
						ImplicitTo = (Func<ResultT, SourceT>)CreateDelegate(typeof(Func<ResultT, SourceT>), method, true);
					else if (method.Name == "op_Explicit" && method.ReturnType == resultType && firstParameterType == sourceType)
						ExplicitFrom = (Func<SourceT, ResultT>)CreateDelegate(typeof(Func<SourceT, ResultT>), method, true);
					else if (method.Name == "op_Implicit" && method.ReturnType == resultType && firstParameterType == sourceType)
						ImplicitFrom = (Func<SourceT, ResultT>)CreateDelegate(typeof(Func<SourceT, ResultT>), method, true);
				}

				var knownConversion = default(Delegate);
				if (KnownConversions.TryGetValue(resultToSourceKey, out knownConversion))
				{
					ConvertibleTo = (Func<ResultT, SourceT>)knownConversion;
				}
				if (KnownConversions.TryGetValue(sourceToResultKey, out knownConversion))
				{
					ConvertibleFrom = (Func<SourceT, ResultT>)knownConversion;
				}

#if ENUMHELPER
				if (sourceType == typeof(string) && resultTypeInfo.IsEnum)
				{
					ConvertibleFrom = (Func<SourceT, ResultT>)(object)new Func<string, ResultT>(EnumHelper<ResultT>.Parse);
					ConvertibleTo = (Func<ResultT, SourceT>)(object)new Func<ResultT, string>(EnumHelper<ResultT>.ToName);
				}
				if (sourceTypeInfo.IsEnum  && resultType == typeof(string))
				{
					ConvertibleTo = (Func<ResultT, SourceT>)(object)new Func<string, SourceT>(EnumHelper<SourceT>.Parse);
					ConvertibleFrom = (Func<SourceT, ResultT>)(object)new Func<SourceT, string>(EnumHelper<SourceT>.ToName);
				}
#endif

#if !NETSTANDARD
				Converter = TypeDescriptor.GetConverter(sourceType);
				if (Converter != null && Converter.GetType() == typeof(TypeConverter))
					Converter = null;
#endif
			}
		}

		private static class TypeConversion<T>
		{
			// nullable
			public static readonly bool CanBeNull;
			public static readonly bool IsNullableValueType;
			public static readonly bool IsString;
			public static readonly bool IsValueType;
			public static readonly bool IsSupportFormatting;
#if !NETSTANDARD
			public static readonly TypeConverter Converter;
#endif
			public static readonly Func<string, T> ParseFn;
			public static readonly Func<string, IFormatProvider, T> ParseFormattedFn;
			public static readonly Func<T, string> ToStringFn;
			public static readonly Func<T, string, IFormatProvider, string> ToStringFormattedFn;
			public static readonly string DefaultFormat;

			static TypeConversion()
			{
				var type = typeof(T);
#if !NETSTANDARD
				var typeInfo = type;
#else
				var typeInfo = type.GetTypeInfo();
#endif
				IsValueType = typeInfo.IsValueType;
				CanBeNull = IsValueType == false;

				if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					IsNullableValueType = true;
					CanBeNull = true;
					return;
				}

				if (typeInfo.IsEnum)
				{
#if !NETSTANDARD
					Converter = new EnumConverter(type);
#endif
					return;
				}

				IsString = typeof(string) == type;
#if !NETSTANDARD
				IsSupportFormatting = Array.IndexOf(type.GetInterfaces(), typeof(IFormattable)) != -1;
#else
				IsSupportFormatting = typeInfo.ImplementedInterfaces.Contains(typeof(IFormattable));
#endif

#if !NETSTANDARD
				Converter = TypeDescriptor.GetConverter(type);
				if (Converter != null && Converter.GetType() == typeof(TypeConverter))
					Converter = null;
#endif

				if (type == typeof(float) || type == typeof(double))
					DefaultFormat = "R";

				foreach (var method in GetPublicMethods(typeInfo, declaredOnly: true))
				{
					if (method.IsStatic && (method.Name == "Parse" || method.Name == "Create") && method.ReturnType == type)
					{
						var parseParams = method.GetParameters();
						if (parseParams.Length == 1 && parseParams[0].ParameterType == typeof(string))
							ParseFn = (Func<string, T>)CreateDelegate(typeof(Func<string, T>), method, true);
						else if (parseParams.Length == 2 && parseParams[0].ParameterType == typeof(string) && parseParams[1].ParameterType == typeof(IFormatProvider))
							ParseFormattedFn = (Func<string, IFormatProvider, T>)CreateDelegate(typeof(Func<string, IFormatProvider, T>), method, true);
					}
					else if (method.IsStatic == false && method.Name == "ToString" && method.ReturnType == typeof(string))
					{
						var toStringParams = method.GetParameters();
						if (toStringParams.Length == 0)
							ToStringFn = (Func<T, string>)CreateDelegate(typeof(Func<T, string>), null, method, false);
						else if (toStringParams.Length == 2 && toStringParams[0].ParameterType == typeof(string) && toStringParams[1].ParameterType == typeof(IFormatProvider))
							ToStringFormattedFn = (Func<T, string, IFormatProvider, string>)CreateDelegate(typeof(Func<T, string, IFormatProvider, string>), null, method, false);
					}
				}
			}
		}

		private static readonly Dictionary<long, ConvertUntypedDelegate> CachedGenericConvertMethods;
		private static readonly Dictionary<long, Delegate> KnownConversions;
		private static readonly MethodInfo ConvertMethodDefinition;
		private static readonly MethodInfo ConvertFromNullableToNullableMethodDefinition;
		private static readonly MethodInfo ConvertFromNullableMethodDefinition;
		private static readonly MethodInfo ConvertToNullableMethodDefinition;
		private static readonly MethodInfo ConvertFromObjectMethodDefinition;
		private static readonly MethodInfo ConvertToObjectMethodDefinition;
		private static readonly MethodInfo ConvertFromEnumToEnumMethodDefinition;
		private static readonly MethodInfo ConvertToEnumMethodDefinition;
		private static readonly MethodInfo ConvertFromEnumMethodDefinition;
		public static readonly IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;

		static TypeConvert()
		{
			// ReSharper disable HeapView.DelegateAllocation
			ConvertMethodDefinition = GetMethodInfo(new Func<object, string, IFormatProvider, object>(Convert<object, object>), getMethodDefinition: true);
			ConvertFromNullableToNullableMethodDefinition = GetMethodInfo(new Func<int?, string, IFormatProvider, int?>(ConvertFromNullableToNullable<int, int>), getMethodDefinition: true);
			ConvertFromNullableMethodDefinition = GetMethodInfo(new Func<int?, string, IFormatProvider, int>(ConvertFromNullable<int, int>), getMethodDefinition: true);
			ConvertToNullableMethodDefinition = GetMethodInfo(new Func<int, string, IFormatProvider, int?>(ConvertToNullable<int, int>), getMethodDefinition: true);
			ConvertFromObjectMethodDefinition = GetMethodInfo(new Func<object, string, IFormatProvider, int>(ConvertFromObject<int>), getMethodDefinition: true);
			ConvertToObjectMethodDefinition = GetMethodInfo(new Func<int, string, IFormatProvider, object>(ConvertToObject), getMethodDefinition: true);
			ConvertFromEnumToEnumMethodDefinition = GetMethodInfo(new Func<ConsoleColor, string, IFormatProvider, ConsoleColor>(ConvertFromEnumToEnum<ConsoleColor, int, ConsoleColor, int>), getMethodDefinition: true);
			ConvertToEnumMethodDefinition = GetMethodInfo(new Func<int, string, IFormatProvider, ConsoleColor>(ConvertToEnum<ConsoleColor, int, int>), getMethodDefinition: true);
			ConvertFromEnumMethodDefinition = GetMethodInfo(new Func<ConsoleColor, string, IFormatProvider, int>(ConvertFromEnum<int, ConsoleColor, int>), getMethodDefinition: true);
			// ReSharper restore HeapView.DelegateAllocation

			CachedGenericConvertMethods = new Dictionary<long, ConvertUntypedDelegate>();
			KnownConversions = new Dictionary<long, Delegate>();

			var convertMethods = default(IEnumerable<MethodInfo>);
#if !NETSTANDARD
			convertMethods = GetPublicMethods(typeof(Convert), declaredOnly: true);
#else
			convertMethods = GetPublicMethods(typeof(Convert).GetTypeInfo(), declaredOnly: true);
#endif
			foreach (var method in convertMethods)
			{
				if (method.IsStatic == false)
					continue;

				var parameters = method.GetParameters();
				if (method.Name.StartsWith("To", StringComparison.Ordinal) == false ||
					method.GetParameters().Length != 1 ||
					method.ReturnType == typeof(void))
				{
					continue;
				}

				var fromType = method.GetParameters()[0].ParameterType;
				var toType = method.ReturnType;
				var conversionFunc = CreateDelegate(typeof(Func<,>).MakeGenericType(fromType, toType), method, throwOnBindingFailure: false);
				var conversionKey = GetTypePairKey(fromType, toType);
				KnownConversions[conversionKey] = conversionFunc;
			}
		}

		public static ToType Convert<FromType, ToType>(FromType value, string format = null, IFormatProvider formatProvider = null)
		{
			var transition = TypeConversion<FromType, ToType>.Transition;
			return transition != null ? transition(value, format, formatProvider) : InternalConvert<FromType, ToType>(value, format, formatProvider);
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
			var cacheKey = GetTypePairKey(fromType, toType);

			var gotFromCache = false;
			lock (CachedGenericConvertMethods)
				gotFromCache = CachedGenericConvertMethods.TryGetValue(cacheKey, out convertFn);

			if (gotFromCache)
				return convertFn.Invoke(value, format, formatProvider);

			var conversionMethod = ConvertMethodDefinition.MakeGenericMethod(fromType, toType);
			var valueParam = Expression.Parameter(typeof(object), "value");
			var formatParam = Expression.Parameter(typeof(string), "format");
			var formatProviderParam = Expression.Parameter(typeof(IFormatProvider), "formatProvider");
			var conversionExpression = Expression.Lambda<ConvertUntypedDelegate>
			(
				Expression.Convert
					(
						Expression.Call
						(
							conversionMethod,
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
				CachedGenericConvertMethods[cacheKey] = convertFn = conversionExpression.Compile();

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
			var sourceIsValueType = TypeConversion<FromType>.IsValueType;

			if (sourceIsValueType)
			{
				sourceIsNullRef = false;
				if (typeof(FromType) == typeof(ToType))
				{
#if NO_TYPE_REFS
					return (ToType)(object)value;
#else
					return __refvalue(__makeref(value), ToType);
#endif
				}
			}
			else
			{
				sourceObj = value;
				sourceIsNullRef = sourceObj == null;

				if (sourceObj is ToType)
					return (ToType)sourceObj;
			}

			// find explicit/implicit conversions between types
			if (TypeConversion<FromType, ToType>.ImplicitFrom != null)
				return TypeConversion<FromType, ToType>.ImplicitFrom(value);
			if (TypeConversion<ToType, FromType>.ImplicitTo != null)
				return TypeConversion<ToType, FromType>.ImplicitTo(value);
			if (TypeConversion<FromType, ToType>.ExplicitFrom != null)
				return TypeConversion<FromType, ToType>.ExplicitFrom(value);
			if (TypeConversion<ToType, FromType>.ExplicitTo != null)
				return TypeConversion<ToType, FromType>.ExplicitTo(value);
			// try parse
			if (TypeConversion<FromType>.IsString && TypeConversion<ToType>.ParseFormattedFn != null)
				return TypeConversion<ToType>.ParseFormattedFn((string)sourceObj, formatProvider);
			if (TypeConversion<FromType>.IsString && TypeConversion<ToType>.ParseFn != null)
				return TypeConversion<ToType>.ParseFn((string)sourceObj);
			// try toString(formatted)
			if (!sourceIsNullRef && TypeConversion<ToType>.IsString && TypeConversion<FromType>.ToStringFormattedFn != null)
				return (ToType)(object)TypeConversion<FromType>.ToStringFormattedFn(value, format ?? TypeConversion<FromType>.DefaultFormat, formatProvider);
			if (!sourceIsNullRef && TypeConversion<ToType>.IsString && TypeConversion<FromType>.IsSupportFormatting)
				// ReSharper disable once PossibleNullReferenceException
				return (ToType)(object)((sourceIsValueType ? (object)value : sourceObj) as IFormattable).ToString(format ?? TypeConversion<FromType>.DefaultFormat, formatProvider);
			// try method conversions (To*\From* methods, Convert.To*)
			if (TypeConversion<ToType, FromType>.ConvertibleTo != null)
				return TypeConversion<ToType, FromType>.ConvertibleTo(value);
			if (TypeConversion<FromType, ToType>.ConvertibleFrom != null)
				return TypeConversion<FromType, ToType>.ConvertibleFrom(value);
#if !NETSTANDARD
			// try converter			
			if (TypeConversion<ToType>.Converter != null && TypeConversion<ToType>.Converter.CanConvertFrom(typeof(FromType)))
				return (ToType)TypeConversion<ToType>.Converter.ConvertFrom(null, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, (sourceIsValueType ? value : sourceObj));
			if (TypeConversion<FromType, ToType>.Converter != null && TypeConversion<FromType, ToType>.Converter.CanConvertTo(typeof(ToType)))
				return (ToType)TypeConversion<FromType, ToType>.Converter.ConvertTo(null, formatProvider as CultureInfo, (sourceIsValueType ? value : sourceObj), typeof(ToType));
#endif
			// try ToString
			if (!sourceIsNullRef && TypeConversion<ToType>.IsString && TypeConversion<FromType>.ToStringFn != null)
				return (ToType)(object)TypeConversion<FromType>.ToStringFn(value);
			if (!sourceIsNullRef && TypeConversion<ToType>.IsString)
				return (ToType)(object)(sourceIsValueType ? (object)value : sourceObj).ToString(); // (5) box value-type

			throw new InvalidCastException(string.Format("Unable to convert value '{2}' of type '{1}' to requested type '{0}'", typeof(ToType), typeof(FromType), ((object)value) ?? "<null>"));
		}

		// transitions
		private static ToType? ConvertFromNullableToNullable<ToType, FromType>(FromType? value, string format, IFormatProvider formatProvider)
			where FromType : struct
			where ToType : struct
		{
			if (value.HasValue == false)
				return default(ToType?);

			var result = Convert<FromType, ToType>(value.Value, format, formatProvider);
			return result;
		}
		private static ToType ConvertFromNullable<ToType, FromType>(FromType? value, string format, IFormatProvider formatProvider)
			where FromType : struct
		{
			if (value.HasValue == false)
			{
				if (TypeConversion<ToType>.CanBeNull)
					return default(ToType);
				throw new InvalidCastException(string.Format("Unable to convert <null> to '{0}'.", typeof(ToType)));
			}

			var result = Convert<FromType, ToType>(value.GetValueOrDefault(), format, formatProvider);
			return result;
		}
		private static ToType? ConvertToNullable<ToType, FromType>(FromType value, string format, IFormatProvider formatProvider)
			where ToType : struct
		{
			if (TypeConversion<FromType>.CanBeNull && ReferenceEquals(value, null))
				return default(ToType?);

			var result = Convert<FromType, ToType>(value, format, formatProvider);
			return result;
		}
		private static ToType ConvertFromObject<ToType>(object value, string format, IFormatProvider formatProvider)
		{
			if (value != null && value.GetType() == typeof(object))
				throw new InvalidCastException();

			if (value == null)
			{
				if (!TypeConversion<ToType>.IsValueType || TypeConversion<ToType>.IsNullableValueType)
					return default(ToType);
				throw new InvalidCastException(string.Format("Unable to convert <null> to '{0}'.", typeof(ToType)));
			}
			return (ToType)Convert(value.GetType(), typeof(ToType), value, format, formatProvider);
		}
		private static object ConvertToObject<FromType>(FromType value, string format, IFormatProvider formatProvider)
		{
			return value;
		}

		private static ToEnumT ConvertFromEnumToEnum<ToEnumT, ToBaseT, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var valueNumber = ((Func<FromEnumT, FromBaseT>)EnumHelper<FromEnumT>.ToNumber).Invoke(value);
			var resultNumber = InternalConvert<FromBaseT, ToBaseT>(valueNumber, format, formatProvider);
			return ((Func<ToBaseT, ToEnumT>)EnumHelper<ToEnumT>.FromNumber).Invoke(resultNumber);
#else
			var valueNumber = (FromBaseT)System.Convert.ChangeType(value, typeof(FromBaseT));
			var resultNumber = InternalConvert<FromBaseT, ToBaseT>(valueNumber, format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), resultNumber);
#endif
		}
		private static ToEnumT ConvertToEnum<ToEnumT, ToBaseT, FromType>(FromType value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var resultNumber = InternalConvert<FromType, ToBaseT>(value, format, formatProvider);
			return ((Func<ToBaseT, ToEnumT>)EnumHelper<ToEnumT>.FromNumber).Invoke(resultNumber);
#else
			var resultNumber = InternalConvert<FromType, ToBaseT>(value, format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), resultNumber);
#endif
		}
		private static ToType ConvertFromEnum<ToType, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var valueNumber = ((Func<FromEnumT, FromBaseT>)EnumHelper<FromEnumT>.ToNumber).Invoke(value);
			var resultNumber = InternalConvert<FromBaseT, ToType>(valueNumber, format, formatProvider);
			return resultNumber;
#else
			var valueNumber = (FromBaseT)System.Convert.ChangeType(value, typeof(FromBaseT));
			var result = InternalConvert<FromBaseT, ToType>(valueNumber, format, formatProvider);
			return result;
#endif
		}

		// reflection helpers
		private static Delegate CreateDelegate(Type delegateType, MethodInfo method, bool throwOnBindingFailure = true)
		{
			if (delegateType == null) throw new ArgumentNullException("delegateType");
			if (method == null) throw new ArgumentNullException("method");

#if !NETSTANDARD
			return Delegate.CreateDelegate(delegateType, method, throwOnBindingFailure);
#else
			try
			{
				return method.CreateDelegate(delegateType);
			}
			catch
			{
				if (throwOnBindingFailure)
					throw;
				else
					return null;
			}

#endif
		}
		private static Delegate CreateDelegate(Type delegateType, object target, MethodInfo method,bool throwOnBindingFailure = true)
		{
			if (delegateType == null) throw new ArgumentNullException("delegateType");
			if (method == null) throw new ArgumentNullException("method");

#if !NETSTANDARD
			return Delegate.CreateDelegate(delegateType, target, method, throwOnBindingFailure);
#else
			try
			{
				return method.CreateDelegate(delegateType, target);
			}
			catch
			{
				if (throwOnBindingFailure)
					throw;
				else
					return null;
			}

#endif
		}
		private static MethodInfo GetMethodInfo(Delegate delegateInstance, bool getMethodDefinition = false)
		{
			if (delegateInstance == null) throw new ArgumentNullException("delegateInstance");

			var methodInfo = default(MethodInfo);
#if !NETSTANDARD
			methodInfo = delegateInstance.Method;
#else
			methodInfo = delegateInstance.GetMethodInfo();
#endif
			if (getMethodDefinition && methodInfo.IsGenericMethod && methodInfo.IsGenericMethodDefinition == false)
				return methodInfo.GetGenericMethodDefinition();

			return methodInfo;
		}
#if !NETSTANDARD
		private static IEnumerable<MethodInfo> GetPublicMethods(Type type, bool declaredOnly)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (declaredOnly)
				return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			else
				return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}
#else
		private static IEnumerable<MethodInfo> GetPublicMethods(TypeInfo typeInfo, bool declaredOnly)
		{
			do
			{
				foreach (var method in typeInfo.DeclaredMethods)
				{
					if (method.IsPublic == false)
						continue;
					yield return method;
				}

				if (declaredOnly)
					break;

				typeInfo = typeInfo.BaseType == null || typeInfo.BaseType == typeof(object) ? null : typeInfo.BaseType.GetTypeInfo();
			} while (typeInfo != null);
		}
#endif
		private static long GetTypePairKey(Type fromType, Type toType)
		{
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));
			if (toType == null) throw new ArgumentNullException(nameof(toType));

			var fromHash = fromType.GetHashCode(); // it's not hashcode, it's an unique sync-lock of type-object
			var toHash = toType.GetHashCode();
			return unchecked(((long)fromHash << 32) | (uint)toHash);
		}
	}
}
