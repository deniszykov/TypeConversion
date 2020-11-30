using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace deniszykov.TypeConversion
{
	internal static class ReflectionExtensions
	{
		public static DelegateT CreateDelegate<DelegateT>(object thisObject, MethodInfo methodInfo, bool throwOnBindFailure = true) where DelegateT : Delegate
		{
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

#if NETSTANDARD
			try
			{
				return (DelegateT)methodInfo.CreateDelegate(typeof(DelegateT), thisObject);
			}
			catch
			{
				if (throwOnBindFailure)
					throw;
				else
					return null;
			}
#else
			return (DelegateT)Delegate.CreateDelegate(typeof(DelegateT), thisObject, methodInfo, throwOnBindFailure);
#endif
		}


#if !NETSTANDARD
		public static IEnumerable<MethodInfo> GetPublicMethods(Type type, bool declaredOnly)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | (declaredOnly ? BindingFlags.DeclaredOnly : 0));

			return methods;
		}
		public static IEnumerable<ConstructorInfo> GetPublicConstructors(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}
#else
		public static IEnumerable<MethodInfo> GetPublicMethods(Type type, bool declaredOnly)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			do
			{
				foreach (var method in type.GetTypeInfo().DeclaredMethods)
				{
					if (method.IsPublic == false)
						continue;
					yield return method;
				}

				if (declaredOnly)
					break;

				type = type.GetTypeInfo().BaseType == null || type.GetTypeInfo().BaseType == typeof(object) ? null : type.GetTypeInfo().BaseType;
			} while (type != null);
		}
		public static IEnumerable<ConstructorInfo> GetPublicConstructors(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			foreach (var constructor in type.GetTypeInfo().DeclaredConstructors)
			{
				if (constructor.IsPublic == false)
					continue;

				yield return constructor;
			}
		}
#endif

#if NETFRAMEWORK
		public static Type GetTypeInfo(this Type type)
		{
			return type;
		}
		public static Type AsType(this Type type)
		{
			return type;
		}
#endif
		public static IEnumerable<Type> EnumerateBaseTypesAndInterfaces(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			var baseType = type.GetTypeInfo();
			while (baseType != null)
			{
				yield return baseType.AsType();
				foreach (var interfaceType in baseType.GetInterfaces())
					yield return interfaceType;
				baseType = baseType.BaseType.GetTypeInfo();
			}
		}
	}
}
