/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System
{
	/// <summary>
	/// Utility class for object construction. A fast and less versatile alternative to <see cref="Activator"/> class.
	/// This class build upon <see cref="Expression"/>'s dynamic code compilation feature and could construct any type without much overhead.
	/// Value type constructor is optimized so no extra boxing occurs on construction. Array types are optimized too and same instance (empty array) returned on each call.
	/// </summary>
	public static class TypeActivator
	{
		private struct ConstructorSignature : IEquatable<ConstructorSignature>
		{
			private readonly Type type;
			private readonly Type arg1Type;
			private readonly Type arg2Type;
			private readonly Type arg3Type;
			private readonly Type arg4Type;

			public ConstructorSignature(Type type, Type arg1Type, Type arg2Type = null, Type arg3Type = null, Type arg4Type = null)
			{
				if (type == null) throw new ArgumentNullException("type");
				if (arg1Type == null) throw new ArgumentNullException("arg1Type");

				this.type = type;
				this.arg1Type = arg1Type;
				this.arg2Type = arg2Type ?? typeof(void);
				this.arg3Type = arg3Type ?? typeof(void);
				this.arg4Type = arg4Type ?? typeof(void);
			}

			public override int GetHashCode()
			{
				return unchecked(this.type.GetHashCode() + this.arg1Type.GetHashCode() + this.arg2Type.GetHashCode() + this.arg3Type.GetHashCode() + this.arg4Type.GetHashCode());
			}
			public override bool Equals(object obj)
			{
				if (obj == null || !(obj is ConstructorSignature))
					return false;
				return this.Equals((ConstructorSignature)obj);
			}
			public bool Equals(ConstructorSignature other)
			{
				return this.type == other.type && this.arg1Type == other.arg1Type && this.arg2Type == other.arg2Type && this.arg3Type == other.arg3Type && this.arg4Type == other.arg4Type;
			}

			public override string ToString() { return string.Format("{0}({1}, {2}, {3}, {4})", this.type.Name, this.arg1Type.Name, this.arg2Type.Name, this.arg3Type.Name, this.arg4Type.Name); }
		}

		private static readonly Dictionary<Type, Func<object>> DefaultConstructorCache = new Dictionary<Type, Func<object>>();
		private static readonly Dictionary<ConstructorSignature, Delegate> CustomConstructorCache = new Dictionary<ConstructorSignature, Delegate>();
		private static readonly HashSet<string> ConstructorSubstitutionMembers = new HashSet<string>(new[] { "Empty", "Default", "Instance" }, StringComparer.OrdinalIgnoreCase);

#if !NETSTANDARD
		/// <summary>
		/// Create an instance of <paramref name="type"/> using empty constructor.
		/// </summary>
		/// <param name="type">Type of object to create.</param>
		/// <returns>An instance of <paramref name="type"/>. If <paramref name="type"/> is value type or array type then same instance is returned every call.</returns>
		public static object CreateInstance(Type type)
		{
			return CreateInstance(type, forceCreate: false);
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using empty constructor.
		/// </summary>
		/// <param name="type">Type of object to create.</param>
		/// <param name="forceCreate">True to create instance of <paramref name="type"/> even if no empty constructor is available (using <see cref="Runtime.Serialization.FormatterServices.GetSafeUninitializedObject"/> API).</param>
		/// <returns>An instance of <paramref name="type"/>. If <paramref name="type"/> is value type or array type then same instance is returned every call.</returns>
		public static object CreateInstance(Type type, bool forceCreate)
#else
		/// <summary>
		/// Create an instance of <paramref name="type"/> using empty constructor.
		/// </summary>
		/// <param name="type">Type of object to create.</param>
		/// <returns>An instance of <paramref name="type"/>. If <paramref name="type"/> is value type or array type then same instance is returned every call.</returns>
		public static object CreateInstance(Type type)
#endif
		{
			if (type == null) throw new ArgumentNullException("type");

			var constructorFn = default(Func<object>);
			lock (DefaultConstructorCache)
			{
				if (DefaultConstructorCache.TryGetValue(type, out constructorFn) == false)
				{
#if !NETSTANDARD
					var typeInfo = type;
					var constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					var publicEmptyConstructor = constructors.SingleOrDefault(c => c.GetParameters().Length == 0 && c.IsPublic && c.IsStatic == false);
					var privateEmptyConstructor = constructors.SingleOrDefault(c => c.GetParameters().Length == 0 && !c.IsPublic && c.IsStatic == false);
					var instanceField = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(f =>
						ConstructorSubstitutionMembers.Contains(f.Name) &&
						type.IsAssignableFrom(f.FieldType)
					);
					var instanceProperty = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(p =>
						ConstructorSubstitutionMembers.Contains(p.Name) &&
						type.IsAssignableFrom(p.PropertyType) &&
						p.CanRead && p.GetIndexParameters().Length == 0
					);
#else
					var typeInfo = type.GetTypeInfo();
					var constructors = typeInfo.DeclaredConstructors.ToList();
					var publicEmptyConstructor = constructors.SingleOrDefault(c => c.GetParameters().Length == 0 && c.IsPublic && c.IsStatic == false);
					var privateEmptyConstructor = constructors.SingleOrDefault(c => c.GetParameters().Length == 0 && c.IsPublic == false && c.IsStatic == false);
					var instanceField = type.GetRuntimeFields().FirstOrDefault(f =>
						f.IsStatic &&
						ConstructorSubstitutionMembers.Contains(f.Name) &&
						IsAssignableFrom(type, f.FieldType)
					);
					var instanceProperty = type.GetRuntimeProperties().FirstOrDefault(p =>
						p.GetMethod != null &&
						p.GetMethod.IsStatic &&
						ConstructorSubstitutionMembers.Contains(p.Name) &&
						IsAssignableFrom(type, p.PropertyType) &&
						p.GetIndexParameters().Length == 0
					);
#endif

					if (publicEmptyConstructor != null)
					{
						var createNewObjectExpr = Expression.Lambda<Func<object>>
						(
							Expression.Convert
							(
								Expression.New(publicEmptyConstructor),
								typeof(object)
							)
						);

						constructorFn = createNewObjectExpr.Compile();
					}
					else if (instanceField != null)
					{
						var getInstanceFieldExpr = Expression.Lambda<Func<object>>
						(
							Expression.Convert
							(
								Expression.Field(null, instanceField),
								typeof(object)
							)
						);
						constructorFn = getInstanceFieldExpr.Compile();
					}
					else if (instanceProperty != null)
					{
						var getInstancePropertyExpr = Expression.Lambda<Func<object>>
						(
							Expression.Convert
							(
								Expression.Property(null, instanceProperty),
								typeof(object)
							)
						);
						constructorFn = getInstancePropertyExpr.Compile();
					}
					else if (privateEmptyConstructor != null)
					{
						var createNewObjectExpr = Expression.Lambda<Func<object>>
						(
							Expression.Convert
							(
								Expression.New(privateEmptyConstructor),
								typeof(object)
							)
						);

						constructorFn = createNewObjectExpr.Compile();
					}
					else if (type.IsArray && type.GetArrayRank() == 1)
					{
						var elementType = type.GetElementType();
						Debug.Assert(elementType != null, "elementType != null");
						var createNewArrayExpr = Expression.Lambda<Func<object>>
						(
							Expression.Convert
							(
								Expression.Constant(Array.CreateInstance(elementType, 0)),
								typeof(object)
							)
						);

						constructorFn = createNewArrayExpr.Compile();
					}
					else if (typeInfo.IsValueType)
					{
						var createNewValueTypeExpr = Expression.Lambda<Func<object>>
						(
							Expression.Constant
							(
								Activator.CreateInstance(type),
								typeof(object)
							)
						);

						constructorFn = createNewValueTypeExpr.Compile();
					}

					DefaultConstructorCache[type] = constructorFn;
				}
			}

#if !NETSTANDARD
			if (constructorFn == null && forceCreate)
				return Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(type);
#endif
			if (constructorFn == null)
				throw new ArgumentException(string.Format("Type '{0}' does not contains default empty constructor.", type), "type");
			else
				return constructorFn();
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using constructor with one argument. First matching constructor is used.
		/// </summary>
		/// <typeparam name="Arg1T">First argument type.</typeparam>
		/// <param name="type">Type of object to create.</param>
		/// <param name="arg1">Value of first argument.</param>
		/// <returns>An instance of <paramref name="type"/>.</returns>
		public static object CreateInstance<Arg1T>(Type type, Arg1T arg1)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance<Arg1T, object, object, object>(type, arg1, null, null, null, 1);
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using constructor with two argument. First matching constructor is used.
		/// </summary>
		/// <typeparam name="Arg1T">First argument type.</typeparam>
		/// <typeparam name="Arg2T">Second argument type.</typeparam>
		/// <param name="type">Type of object to create.</param>
		/// <param name="arg1">Value of first argument.</param>
		/// <param name="arg2">Value for second argument.</param>
		/// <returns>An instance of <paramref name="type"/>.</returns>
		public static object CreateInstance<Arg1T, Arg2T>(Type type, Arg1T arg1, Arg2T arg2)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance<Arg1T, Arg2T, object, object>(type, arg1, arg2, null, null, 2);
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using constructor with three argument. First matching constructor is used.
		/// </summary>
		/// <typeparam name="Arg1T">First argument type.</typeparam>
		/// <typeparam name="Arg2T">Second argument type.</typeparam>
		/// <typeparam name="Arg3T">Third argument type.</typeparam>
		/// <param name="type">Type of object to create.</param>
		/// <param name="arg1">Value of first argument.</param>
		/// <param name="arg2">Value for second argument.</param>
		/// <param name="arg3">Value for third argument.</param>
		/// <returns>An instance of <paramref name="type"/>.</returns>
		public static object CreateInstance<Arg1T, Arg2T, Arg3T>(Type type, Arg1T arg1, Arg2T arg2, Arg3T arg3)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance<Arg1T, Arg2T, Arg3T, object>(type, arg1, arg2, arg3, null, 3);
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using constructor with four argument. First matching constructor is used.
		/// </summary>
		/// <typeparam name="Arg1T">First argument type.</typeparam>
		/// <typeparam name="Arg2T">Second argument type.</typeparam>
		/// <typeparam name="Arg3T">Third argument type.</typeparam>
		/// <typeparam name="Arg4T">Forth argument type.</typeparam>
		/// <param name="type">Type of object to create.</param>
		/// <param name="arg1">Value of first argument.</param>
		/// <param name="arg2">Value for second argument.</param>
		/// <param name="arg3">Value for third argument.</param>
		/// <param name="arg4">Value for forth argument.</param>
		/// <returns>An instance of <paramref name="type"/>.</returns>
		public static object CreateInstance<Arg1T, Arg2T, Arg3T, Arg4T>(Type type, Arg1T arg1, Arg2T arg2, Arg3T arg3, Arg4T arg4)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance(type, arg1, arg2, arg3, arg4, 4);
		}
		/// <summary>
		/// Create an instance of <paramref name="type"/> using constructor with four argument. First matching constructor is used.
		/// </summary>
		/// <typeparam name="Arg1T">First argument type.</typeparam>
		/// <typeparam name="Arg2T">Second argument type.</typeparam>
		/// <typeparam name="Arg3T">Third argument type.</typeparam>
		/// <typeparam name="Arg4T">Forth argument type.</typeparam>
		/// <param name="type">Type of object to create.</param>
		/// <param name="arg1">Value of first argument.</param>
		/// <param name="arg2">Value for second argument.</param>
		/// <param name="arg3">Value for third argument.</param>
		/// <param name="arg4">Value for forth argument.</param>
		/// <param name="argCount">Number of arguments.</param>
		/// <returns>An instance of <paramref name="type"/>.</returns>
		private static object CreateInstance<Arg1T, Arg2T, Arg3T, Arg4T>(Type type, Arg1T arg1, Arg2T arg2, Arg3T arg3, Arg4T arg4, int argCount)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (argCount < 1 || argCount > 4) throw new ArgumentOutOfRangeException("argCount");

			var signature = new ConstructorSignature(type, 
				typeof(Arg1T), argCount > 1 ? 
				typeof(Arg2T) : null, argCount > 2 ? 
				typeof(Arg3T) : argCount > 3 ? 
				typeof(Arg4T) : null);
#if !NETSTANDARD
			var typeInfo = type;
			var constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
			var typeInfo = type.GetTypeInfo();
			var constructors = typeInfo.DeclaredConstructors.ToList();
#endif
			var constructor = default(Delegate);
			lock (CustomConstructorCache)
			{
				if (CustomConstructorCache.TryGetValue(signature, out constructor) == false)
				{
					var foundCtr = default(ConstructorInfo);

					foreach (var constructorInfo in constructors)
					{
						var ctrParams = constructorInfo.GetParameters();
						if (ctrParams.Length != argCount)
							continue;
						if (argCount > 0 && IsAssignableFrom(ctrParams[0].ParameterType, typeof(Arg1T)) == false)
							continue;
						if (argCount > 1 && IsAssignableFrom(ctrParams[1].ParameterType, typeof(Arg2T)) == false)
							continue;
						if (argCount > 2 && IsAssignableFrom(ctrParams[2].ParameterType, typeof(Arg3T)) == false)
							continue;
						if (argCount > 3 && IsAssignableFrom(ctrParams[3].ParameterType, typeof(Arg4T)) == false)
							continue;

						foundCtr = constructorInfo;
						break;
					}

					if (foundCtr != null)
					{
						var ctrParameters = foundCtr.GetParameters();
						var arg1Param = Expression.Parameter(typeof(Arg1T), "arg1");
						var arg2Param = Expression.Parameter(typeof(Arg2T), "arg2");
						var arg3Param = Expression.Parameter(typeof(Arg3T), "arg3");
						var arg4Param = Expression.Parameter(typeof(Arg4T), "arg4");
						switch (argCount)
						{
							case 1:
								constructor = Expression.Lambda<Func<Arg1T, object>>
									(
										Expression.Convert(
											Expression.New(
												foundCtr,
												Expression.Convert(arg1Param, ctrParameters[0].ParameterType)),
											typeof(object)
											),
										arg1Param
									).Compile();
								break;
							case 2:
								constructor = Expression.Lambda<Func<Arg1T, Arg2T, object>>
									(
										Expression.Convert(
											Expression.New(
												foundCtr,
												Expression.Convert(arg1Param, ctrParameters[0].ParameterType),
												Expression.Convert(arg2Param, ctrParameters[1].ParameterType)),
											typeof(object)
											),
										arg1Param,
										arg2Param
									).Compile();
								break;
							case 3:
								constructor = Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, object>>
									(
										Expression.Convert(
											Expression.New(
												foundCtr,
												Expression.Convert(arg1Param, ctrParameters[0].ParameterType),
												Expression.Convert(arg2Param, ctrParameters[1].ParameterType),
												Expression.Convert(arg3Param, ctrParameters[2].ParameterType)),
											typeof(object)
											),
										arg1Param,
										arg2Param,
										arg3Param
									).Compile();
								break;
							case 4:
								constructor = Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, Arg4T, object>>
								(
									Expression.Convert(
										Expression.New(
											foundCtr,
											Expression.Convert(arg1Param, ctrParameters[0].ParameterType),
											Expression.Convert(arg2Param, ctrParameters[1].ParameterType),
											Expression.Convert(arg3Param, ctrParameters[2].ParameterType),
											Expression.Convert(arg4Param, ctrParameters[3].ParameterType)),
										typeof(object)
									),
									arg1Param,
									arg2Param,
									arg3Param,
									arg4Param
								).Compile();
								break;
						}
					}

					CustomConstructorCache[signature] = constructor;
				}
			}

			var instance = default(object);
			// create instance
			switch (argCount)
			{
				case 1:
					if (constructor == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1})'.", type, typeof(Arg1T).Name), "type");
					instance = ((Func<Arg1T, object>)constructor)(arg1);
					break;
				case 2:
					if (constructor == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1}, {2})'.", type, typeof(Arg1T).Name, typeof(Arg2T).Name), "type");
					instance = ((Func<Arg1T, Arg2T, object>)constructor)(arg1, arg2);
					break;
				case 3:
					if (constructor == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1}, {2}, {3})'.", type, typeof(Arg1T).Name, typeof(Arg2T).Name, typeof(Arg3T).Name), "type");
					instance = ((Func<Arg1T, Arg2T, Arg3T, object>)constructor)(arg1, arg2, arg3);
					break;
				case 4:
					if (constructor == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1}, {2}, {3}, {4})'.", type, typeof(Arg1T).Name, typeof(Arg2T).Name, typeof(Arg3T).Name, typeof(Arg4T).Name), "type");
					instance = ((Func<Arg1T, Arg2T, Arg3T, Arg4T, object>)constructor)(arg1, arg2, arg3, arg4);
					break;
			}

			return instance;
		}

		private static bool IsAssignableFrom(Type type, Type fromType)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (fromType == null) throw new ArgumentNullException("fromType");

			if (type == fromType)
				return true;
#if !NETSTANDARD
			return type.IsAssignableFrom(fromType);
#else
			return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
#endif
		}
	}
}
