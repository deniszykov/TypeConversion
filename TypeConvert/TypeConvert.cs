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
	/// <summary>
	/// Utility class for conversion between types. Class uses reflection and heuristics to find a way to convert one type to another.
	/// Explicit/implicit conversion operators, Parse/To/Create methods are located and used for conversion.
	/// </summary>
	public static partial class TypeConvert
	{
		private static class TypeConversion<SourceT, ResultT>
		{
			public static readonly Func<SourceT, ResultT> ConversionFn;

			public static Func<SourceT, string, IFormatProvider, ResultT> NativeConversionFn;

			static TypeConversion()
			{

#if !NETSTANDARD
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

				if (sourceType == resultType)
				{
					NativeConversionFn = (Func<SourceT, string, IFormatProvider, ResultT>)(object)ConvertibleType<SourceT>.IdentityFn;
					return;
				}

				var transitionMethod = GetTransitionMethod(sourceType, resultType);
				if (transitionMethod != null)
				{
					NativeConversionFn = (Func<SourceT, string, IFormatProvider, ResultT>)CreateDelegate(typeof(Func<SourceT, string, IFormatProvider, ResultT>), transitionMethod);
					return;
				}

				var sourceToResultKey = GetTypePairKey(sourceType, resultType);
				var knownConversion = default(Delegate);
#if ENUMHELPER
				if (sourceType == typeof(string) && resultTypeInfo.IsEnum)
				{
					ConversionFn = (Func<SourceT, ResultT>)(object)new Func<string, ResultT>(EnumHelper<ResultT>.Parse);
				}
				else if (sourceTypeInfo.IsEnum && resultType == typeof(string))
				{
					ConversionFn = (Func<SourceT, ResultT>)(object)new Func<ResultT, string>(EnumHelper<ResultT>.ToName);
				}
				else
#endif
				{
					var convertMethodInfoX = default(ConvertMethodInfo);
					var convertMethodInfoY = default(ConvertMethodInfo);
					if (ConvertibleType<SourceT>.TryFindToConversion(resultType, out convertMethodInfoX) ||
						ConvertibleType<ResultT>.TryFindFromConversion(sourceType, out convertMethodInfoY))
					{
						var bestConversionMethod = ConvertMethodInfo.ChooseByQuality(convertMethodInfoY, convertMethodInfoX).Method;
						ConversionFn = (Func<SourceT, ResultT>)CreateDelegate(typeof(Func<SourceT, ResultT>), null, bestConversionMethod);
					}
					else if (KnownConversions.TryGetValue(sourceToResultKey, out knownConversion))
					{
						ConversionFn = (Func<SourceT, ResultT>)knownConversion;
					}
				}
			}

			private static MethodInfo GetTransitionMethod(Type sourceType, Type resultType)
			{
				if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
				if (resultType == null) throw new ArgumentNullException(nameof(resultType));

#if !NETSTANDARD
				var sourceTypeInfo = sourceType;
				var resultTypeInfo = resultType;
#else
				var sourceTypeInfo = sourceType.GetTypeInfo();
				var resultTypeInfo = resultType.GetTypeInfo();
#endif

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

				return transitionMethod;
			}
		}

		private static class ConvertibleType<T>
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
			public static readonly Func<T, string, IFormatProvider, T> IdentityFn;
			public static readonly Func<T, string, IFormatProvider, string> ToStringFormattedFn;
			public static readonly ConvertMethodInfo[] ConvertFrom; // from ANY type to T type
			public static readonly ConvertMethodInfo[] ConvertTo; // from T type to ANY type
			public static readonly string DefaultFormat;

			static ConvertibleType()
			{
				var type = typeof(T);
#if !NETSTANDARD
				var typeInfo = type;
#else
				var typeInfo = type.GetTypeInfo();
#endif
				IsValueType = typeInfo.IsValueType;
				CanBeNull = IsValueType == false;
				ConvertFrom = (ConvertMethodInfo[])Enumerable.Empty<ConvertMethodInfo>();
				ConvertTo = (ConvertMethodInfo[])Enumerable.Empty<ConvertMethodInfo>();
				IdentityFn = Identity;

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

				var convertFrom = default(List<ConvertMethodInfo>);
				var convertTo = default(List<ConvertMethodInfo>);

				foreach (var method in GetPublicMethods(typeInfo, declaredOnly: true))
				{
					var parameters = default(ParameterInfo[]);
					// Parse
					if (IsParseMethod(method, type))
					{
						parameters = parameters ?? method.GetParameters();
						if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
						{
							ParseFn = (Func<string, T>)CreateDelegate(typeof(Func<string, T>), method, true);
						}
						else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(IFormatProvider))
						{
							ParseFormattedFn = (Func<string, IFormatProvider, T>)CreateDelegate(typeof(Func<string, IFormatProvider, T>), method, true);
						}
					}
					// ToString
					else if (IsToStringMethod(method))
					{
						parameters = parameters ?? method.GetParameters();
						if (parameters.Length == 0)
						{
							ToStringFn = (Func<T, string>)CreateDelegate(typeof(Func<T, string>), null, method, false);
						}
						else if (parameters.Length == 2 &&
							parameters[0].ParameterType == typeof(string) &&
							parameters[1].ParameterType == typeof(IFormatProvider))
						{
							ToStringFormattedFn = (Func<T, string, IFormatProvider, string>)CreateDelegate(typeof(Func<T, string, IFormatProvider, string>), null, method, false);
						}
					}

					// Explicit/Implicit operators
					if (method.IsStatic && method.IsSpecialName && (method.Name == "op_Explicit" || method.Name == "op_Implicit"))
					{
						parameters = parameters ?? method.GetParameters();
						if (parameters.Length == 1 && parameters[0].ParameterType == type)
						{
							(convertTo ?? (convertTo = new List<ConvertMethodInfo>())).Add(new ConvertMethodInfo(method, ref parameters));
						}
						else if (parameters.Length == 1 && method.ReturnType == type)
						{
							(convertFrom ?? (convertFrom = new List<ConvertMethodInfo>())).Add(new ConvertMethodInfo(method, ref parameters));
						}
					}

					// custom FromX method
					if (IsConvertFromMethod(method, type, ref parameters))
					{
						(convertFrom ?? (convertFrom = new List<ConvertMethodInfo>())).Add(new ConvertMethodInfo(method, ref parameters));
					}

					// custom ToX method
					if (IsConvertToMethod(method, type, ref parameters))
					{
						(convertTo ?? (convertTo = new List<ConvertMethodInfo>())).Add(new ConvertMethodInfo(method, ref parameters));
					}
				}

				if (convertTo != null)
				{
					ConvertTo = convertTo.ToArray();
					// sort by Quality descending
					Array.Sort(ConvertTo, (x, y) => ((int)y.Quality).CompareTo((int)x.Quality));
				}

				if (convertFrom != null)
				{
					ConvertFrom = convertFrom.ToArray();
					// sort by Quality descending
					Array.Sort(ConvertFrom, (x, y) => ((int)y.Quality).CompareTo((int)x.Quality));
				}
			}

			private static T Identity(T value, string format, IFormatProvider formatProvider)
			{
				return value;
			}

			private static bool IsToStringMethod(MethodInfo method)
			{
				return method.IsStatic == false &&
					method.Name == "ToString" &&
					method.ReturnType == typeof(string);
			}
			private static bool IsParseMethod(MethodInfo method, Type type)
			{
				return method.IsStatic &&
					(method.Name == "Parse" ||
						method.Name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
						method.Name.StartsWith("From", StringComparison.OrdinalIgnoreCase)) &&
					method.ReturnType == type;
			}
			private static bool IsConvertFromMethod(MethodInfo method, Type type, ref ParameterInfo[] parameters)
			{
				return method.IsStatic &&
					(method.Name == "Parse" ||
						method.Name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
						method.Name.StartsWith("From", StringComparison.OrdinalIgnoreCase)) &&
					method.ReturnType == type &&
					method.DeclaringType == type &&
					(parameters ?? (parameters = method.GetParameters())).Length == 1 &&
					IsPlainParameter(parameters[0]);
			}
			private static bool IsConvertToMethod(MethodInfo method, Type type, ref ParameterInfo[] parameters)
			{

				if (method.Name.StartsWith("To", StringComparison.OrdinalIgnoreCase) == false ||
					method.Name == "ToString" ||
					method.DeclaringType != type)
				{
					return false;
				}

				parameters = parameters ?? method.GetParameters();

				return (method.IsStatic ?
					parameters.Length == 1 && parameters[0].ParameterType == type && IsPlainParameter(parameters[0]) :
					parameters.Length == 0);
			}
			private static bool IsPlainParameter(ParameterInfo parameterInfo)
			{
				return parameterInfo.IsIn == false && parameterInfo.IsOut == false && parameterInfo.ParameterType.IsByRef == false;
			}

			public static bool TryFindToConversion(Type toType, out ConvertMethodInfo convertMethod)
			{
				convertMethod = default(ConvertMethodInfo);
				if (ConvertTo.Length == 0)
					return false;

				foreach (var method in ConvertTo)
				{
					if (method.ToType == toType)
					{

						convertMethod = method;
						return true;
					}
				}

				return false;
			}
			public static bool TryFindFromConversion(Type fromType, out ConvertMethodInfo convertMethod)
			{
				convertMethod = default(ConvertMethodInfo);
				if (ConvertFrom.Length == 0)
					return false;

				foreach (var method in ConvertFrom)
				{
					if (method.FromType == fromType)
					{

						convertMethod = method;
						return true;
					}
				}

				return false;
			}
		}

		private class ConvertMethodInfo
		{
			public readonly MethodInfo Method;
			public readonly Type FromType;
			public readonly Type ToType;
			public readonly ConversionQuality Quality;

			public ConvertMethodInfo(MethodInfo method, ref ParameterInfo[] methodParameters)
			{
				if (method == null) throw new ArgumentNullException(nameof(method));

				methodParameters = methodParameters ?? method.GetParameters();
				this.Method = method;
				this.FromType = (method.IsStatic ? methodParameters[0].ParameterType : method.DeclaringType) ?? typeof(object);
				this.ToType = method.ReturnType;
				this.Quality = method.Name == "op_Explicit" ? ConversionQuality.Explicit :
					method.Name == "op_Implicit" ? ConversionQuality.Implicit : ConversionQuality.CustomMethod;
			}

			public static ConvertMethodInfo ChooseByQuality(ConvertMethodInfo x, ConvertMethodInfo y)
			{
				if (x == null && y == null)
					return null;
				else if (x != null && y == null)
					return x;
				else if (x == null && y != null)
					return y;

				if (x.Quality >= y.Quality)
					return x;
				else
					return y;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return string.Format("From: " + this.FromType.Name + ", To: " + this.ToType.Name + ", Method: " + this.Method.Name + ", Quality:" + this.Quality);
			}
		}

		private enum ConversionQuality : byte
		{
			None = 0,
			CustomMethod,
			Explicit,
			Implicit,
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
		/// <summary>
		/// Default format provider used when null formatProvider parameter is passed to <see cref="Convert"/> methods.
		/// </summary>
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

#if NATIVE_CONVERSIONS
			InitializeNativeConversions();
#endif
		}

		/// <summary>
		/// Convert passed <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using explicit/implicit conversion, Parse/ToString methods,
		/// <see cref="IFormattable"/> interface, <see cref="IConvertible"/> interface or From/To methods.
		/// </summary>
		/// <typeparam name="FromType">Type of <paramref name="value"/> object.</typeparam>
		/// <typeparam name="ToType">A result type for <paramref name="value"/> object.</typeparam>
		/// <param name="value">A value to convert to <typeparamref name="ToType"/>.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types. For numbers conversion format accept "checked" <see cref="CheckedConversionFormat"/> or "unchecked" <see cref="UncheckedConversionFormat"/> values.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>A value converted to <typeparamref name="ToType"/> type.</returns>
		public static ToType Convert<FromType, ToType>(FromType value, string format = null, IFormatProvider formatProvider = null)
		{
			if (formatProvider == null) formatProvider = DefaultFormatProvider;

			// native
			if (TypeConversion<FromType, ToType>.NativeConversionFn != null)
			{
				return TypeConversion<FromType, ToType>.NativeConversionFn(value, format, formatProvider);
			}

			var sourceObj = default(object);
			var sourceIsNullRef = default(bool);
			var sourceIsValueType = ConvertibleType<FromType>.IsValueType;

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

			// parse
			if (ConvertibleType<FromType>.IsString && ConvertibleType<ToType>.ParseFormattedFn != null)
				return ConvertibleType<ToType>.ParseFormattedFn((string)sourceObj, formatProvider);
			if (ConvertibleType<FromType>.IsString && ConvertibleType<ToType>.ParseFn != null)
				return ConvertibleType<ToType>.ParseFn((string)sourceObj);

			// toString(formatted)
			if (!sourceIsNullRef && ConvertibleType<ToType>.IsString && ConvertibleType<FromType>.ToStringFormattedFn != null)
				return (ToType)(object)ConvertibleType<FromType>.ToStringFormattedFn(value, format ?? ConvertibleType<FromType>.DefaultFormat, formatProvider);
			if (!sourceIsNullRef && ConvertibleType<ToType>.IsString && ConvertibleType<FromType>.IsSupportFormatting)
				// ReSharper disable once PossibleNullReferenceException
				return (ToType)(object)((sourceIsValueType ? (object)value : sourceObj) as IFormattable).ToString(format ?? ConvertibleType<FromType>.DefaultFormat, formatProvider);

			// find explicit/implicit, To*\From* methods, Convert.To*, Parse/ToString conversions between types
			if (TypeConversion<FromType, ToType>.ConversionFn != null)
				return TypeConversion<FromType, ToType>.ConversionFn(value);

#if !NETSTANDARD
			// try converter
			if (ConvertibleType<ToType>.Converter != null && ConvertibleType<ToType>.Converter.CanConvertFrom(typeof(FromType)))
				return (ToType)ConvertibleType<ToType>.Converter.ConvertFrom(null, formatProvider as CultureInfo ?? CultureInfo.InvariantCulture, (sourceIsValueType ? value : sourceObj));
			if (ConvertibleType<FromType>.Converter != null && ConvertibleType<FromType>.Converter.CanConvertTo(typeof(ToType)))
				return (ToType)ConvertibleType<FromType>.Converter.ConvertTo(null, formatProvider as CultureInfo, (sourceIsValueType ? value : sourceObj), typeof(ToType));
#endif

			// try ToString
			if (!sourceIsNullRef && ConvertibleType<ToType>.IsString && ConvertibleType<FromType>.ToStringFn != null)
				return (ToType)(object)ConvertibleType<FromType>.ToStringFn(value);
			if (!sourceIsNullRef && ConvertibleType<ToType>.IsString)
				return (ToType)(object)(sourceIsValueType ? (object)value : sourceObj).ToString();

			throw new InvalidCastException(string.Format("Unable to convert value '{2}' of type '{1}' to requested type '{0}'", typeof(ToType), typeof(FromType), ((object)value) ?? "<null>"));
		}
		/// <summary>
		/// Try to convert passed <paramref name="value"/> from <typeparamref name="FromType"/> to <typeparamref name="ToType"/> using explicit/implicit conversion, Parse/ToString methods,
		/// <see cref="IFormattable"/> interface, <see cref="IConvertible"/> interface or From/To methods.
		/// </summary>
		/// <typeparam name="FromType">Type of <paramref name="value"/> object.</typeparam>
		/// <typeparam name="ToType">A result type for <paramref name="value"/> object.</typeparam>
		/// <param name="result">A value converted to <typeparamref name="ToType"/> type.</param>
		/// <param name="value">A value to convert to <typeparamref name="ToType"/>.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types. For numbers conversion format accept "checked" <see cref="CheckedConversionFormat"/> or "unchecked" <see cref="UncheckedConversionFormat"/> values.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
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
		/// <summary>
		/// Convert passed <paramref name="value"/> to it's string representation applying <paramref name="format"/> and using <paramref name="formatProvider"/>(defaults to <see cref="CultureInfo.InvariantCulture"/>).
		/// </summary>
		/// <typeparam name="FromType">Type of <paramref name="value"/>.</typeparam>
		/// <param name="value">Value to convert. If null object is passed a null object will returns.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>A string representation of passed <paramref name="value"/>. A null object if <paramref name="value"/> is null.</returns>
		/// <returns></returns>
		public static string ToString<FromType>(FromType value, string format = null, IFormatProvider formatProvider = null)
		{
			return Convert<FromType, string>(value, format, formatProvider);
		}
		/// <summary>
		/// Convert passed <paramref name="value"/> to <paramref name="toType"/> using explicit/implicit conversion, Parse/ToString methods,
		/// <see cref="IFormattable"/> interface, <see cref="IConvertible"/> interface or From/To methods.
		/// </summary>
		/// <param name="toType">A result type for <paramref name="value"/> object.</param>
		/// <param name="value">A value to convert to <paramref name="toType"/>.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types. For numbers conversion format accept "checked" <see cref="CheckedConversionFormat"/> or "unchecked" <see cref="UncheckedConversionFormat"/> values.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>A value converted to <paramref name="toType"/> type.</returns>
		public static object Convert(object value, Type toType, string format = null, IFormatProvider formatProvider = null)
		{
			if (toType == null) throw new ArgumentNullException("toType");

			var fromType = value != null ? value.GetType() : typeof(object);
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
		/// <summary>
		/// Try to convert passed <paramref name="value"/> to <paramref name="toType"/> using explicit/implicit conversion, Parse/ToString methods,
		/// <see cref="IFormattable"/> interface, <see cref="IConvertible"/> interface or From/To methods.
		/// </summary>
		/// <param name="toType">A result type for <paramref name="value"/> object.</param>
		/// <param name="value">A value to convert to <paramref name="toType"/>.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types. For numbers conversion format accept "checked" <see cref="CheckedConversionFormat"/> or "unchecked" <see cref="UncheckedConversionFormat"/> values.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>True if conversion succeed. False if not.</returns>
		public static bool TryConvert(ref object value, Type toType, string format = null, IFormatProvider formatProvider = null)
		{
			try
			{
				value = Convert(value, toType, format, formatProvider);
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
		/// <summary>
		/// Convert passed <paramref name="value"/> to it's string representation applying <paramref name="format"/> and using <paramref name="formatProvider"/>(defaults to <see cref="CultureInfo.InvariantCulture"/>).
		/// </summary>
		/// <param name="value">Value to convert. If null object is passed a <see cref="String.Empty"/> is returned.</param>
		/// <param name="format">Format of string representation. Used with <see cref="IFormattable"/> types. For numbers conversion format accept "checked" <see cref="CheckedConversionFormat"/> or "unchecked" <see cref="UncheckedConversionFormat"/> values.</param>
		/// <param name="formatProvider">Format provider for string representation. Used with <see cref="IFormattable"/> types.</param>
		/// <returns>A string representation of passed <paramref name="value"/>. When <paramref name="value"/> is null then <see cref="String.Empty"/> is returned.</returns>
		public static string ToString(object value, string format = null, IFormatProvider formatProvider = null)
		{
			if (value == null || value is string)
				return (string)value ?? string.Empty;

			return (string)Convert(value, typeof(string), format, formatProvider) ?? string.Empty;
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
				if (ConvertibleType<ToType>.CanBeNull)
					return default(ToType);
				throw new InvalidCastException(string.Format("Unable to convert <null> to '{0}'.", typeof(ToType)));
			}

			var result = Convert<FromType, ToType>(value.GetValueOrDefault(), format, formatProvider);
			return result;
		}
		private static ToType? ConvertToNullable<ToType, FromType>(FromType value, string format, IFormatProvider formatProvider)
			where ToType : struct
		{
			if (ConvertibleType<FromType>.CanBeNull && ReferenceEquals(value, null))
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
				if (!ConvertibleType<ToType>.IsValueType || ConvertibleType<ToType>.IsNullableValueType)
					return default(ToType);
				throw new InvalidCastException(string.Format("Unable to convert <null> to '{0}'.", typeof(ToType)));
			}
			return (ToType)Convert(value, typeof(ToType), format, formatProvider);
		}
		private static object ConvertToObject<FromType>(FromType value, string format, IFormatProvider formatProvider)
		{
			return value;
		}

		private static ToEnumT ConvertFromEnumToEnum<ToEnumT, ToBaseT, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var valueNumber = ((Func<FromEnumT, FromBaseT>)EnumHelper<FromEnumT>.ToNumber).Invoke(value);
			var resultNumber = Convert<FromBaseT, ToBaseT>(valueNumber, format, formatProvider);
			return ((Func<ToBaseT, ToEnumT>)EnumHelper<ToEnumT>.FromNumber).Invoke(resultNumber);
#else
			var valueNumber = (FromBaseT)System.Convert.ChangeType(value, typeof(FromBaseT));
			var resultNumber = Convert<FromBaseT, ToBaseT>(valueNumber, format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), resultNumber);
#endif
		}
		private static ToEnumT ConvertToEnum<ToEnumT, ToBaseT, FromType>(FromType value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var resultNumber = Convert<FromType, ToBaseT>(value, format, formatProvider);
			return ((Func<ToBaseT, ToEnumT>)EnumHelper<ToEnumT>.FromNumber).Invoke(resultNumber);
#else
			var resultNumber = Convert<FromType, ToBaseT>(value, format, formatProvider);
			return (ToEnumT)Enum.ToObject(typeof(ToEnumT), resultNumber);
#endif
		}
		private static ToType ConvertFromEnum<ToType, FromEnumT, FromBaseT>(FromEnumT value, string format, IFormatProvider formatProvider)
		{
#if ENUMHELPER
			var valueNumber = ((Func<FromEnumT, FromBaseT>)EnumHelper<FromEnumT>.ToNumber).Invoke(value);
			var resultNumber = Convert<FromBaseT, ToType>(valueNumber, format, formatProvider);
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
		private static Delegate CreateDelegate(Type delegateType, object target, MethodInfo method, bool throwOnBindingFailure = true)
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
