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
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace System
{
	public static class TypeActivator
	{
		private struct ConstructorSignature : IEquatable<ConstructorSignature>
		{
			private readonly Type Type;
			private readonly Type Arg1Type;
			private readonly Type Arg2Type;
			private readonly Type Arg3Type;

			public ConstructorSignature(Type type, Type arg1Type, Type arg2Type = null, Type arg3Type = null)
			{
				if (type == null) throw new ArgumentNullException("type");
				if (arg1Type == null) throw new ArgumentNullException("arg1Type");

				this.Type = type;
				this.Arg1Type = arg1Type;
				this.Arg2Type = arg2Type ?? typeof(void);
				this.Arg3Type = arg3Type ?? typeof(void);
			}

			public override int GetHashCode()
			{
				return unchecked(Type.GetHashCode() + Arg1Type.GetHashCode() + Arg2Type.GetHashCode() + Arg3Type.GetHashCode());
			}
			public override bool Equals(object obj)
			{
				if (obj == null || !(obj is ConstructorSignature))
					return false;
				return this.Equals((ConstructorSignature)obj);
			}
			public bool Equals(ConstructorSignature other)
			{
				return this.Type == other.Type && this.Arg1Type == other.Arg1Type && this.Arg2Type == other.Arg2Type && this.Arg3Type == other.Arg3Type;
			}

			public override string ToString() { return string.Format("{0}({1}, {2}, {3})", this.Type.Name, this.Arg1Type.Name, this.Arg2Type.Name, this.Arg3Type.Name); }
		}

		private static readonly Dictionary<Type, Func<object>> DefaultConstructorCache = new Dictionary<Type, Func<object>>();
		private static readonly Dictionary<ConstructorSignature, Delegate> CustomConstructorCache = new Dictionary<ConstructorSignature, Delegate>();
		private static readonly HashSet<string> ConstructorSubstitutionMembers = new HashSet<string>(new[] { "Empty", "Default", "Instance" }, StringComparer.OrdinalIgnoreCase);

		public static object CreateInstance(Type type, bool forceCreate = false)
		{
			if (type == null) throw new ArgumentNullException("type");

			var constructorFn = default(Func<object>);
			lock (DefaultConstructorCache)
			{
				if (DefaultConstructorCache.TryGetValue(type, out constructorFn) == false)
				{
					var ctrs = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					var publicEmptyConstructor = ctrs.SingleOrDefault(c => c.GetParameters().Length == 0 && c.IsPublic);
					var privateEmptyConstructor = ctrs.SingleOrDefault(c => c.GetParameters().Length == 0 && !c.IsPublic);
					var instanceField = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(f => ConstructorSubstitutionMembers.Contains(f.Name) && type.IsAssignableFrom(f.FieldType));
					var instanceProperty = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(p => ConstructorSubstitutionMembers.Contains(p.Name) && type.IsAssignableFrom(p.PropertyType) && p.CanRead && p.GetIndexParameters().Length == 0);

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
					else if (type.IsValueType)
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

			if (constructorFn == null && forceCreate)
				return FormatterServices.GetSafeUninitializedObject(type);
			else if (constructorFn == null)
				throw new ArgumentException(string.Format("Type '{0}' does not contains default empty constructor.", type), "type");
			else
				return constructorFn();
		}
		public static object CreateInstance<Arg1T>(Type type, Arg1T arg1)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance<Arg1T, object, object>(type, arg1, null, null, 1);
		}
		public static object CreateInstance<Arg1T, Arg2T>(Type type, Arg1T arg1, Arg2T arg2)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance<Arg1T, Arg2T, object>(type, arg1, arg2, null, 2);
		}
		public static object CreateInstance<Arg1T, Arg2T, Arg3T>(Type type, Arg1T arg1, Arg2T arg2, Arg3T arg3)
		{
			if (type == null) throw new ArgumentNullException("type");

			return CreateInstance(type, arg1, arg2, arg3, 3);
		}
		private static object CreateInstance<Arg1T, Arg2T, Arg3T>(Type type, Arg1T arg1, Arg2T arg2, Arg3T arg3, int argCount)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (argCount < 1 || argCount > 3) throw new ArgumentOutOfRangeException("argCount");

			var signature = new ConstructorSignature(type, typeof(Arg1T), argCount > 1 ? typeof(Arg2T) : null, argCount > 2 ? typeof(Arg3T) : null);

			var ctr = default(Delegate);
			lock (CustomConstructorCache)
			{
				if (CustomConstructorCache.TryGetValue(signature, out ctr) == false)
				{
					var foundCtr = default(ConstructorInfo);

					foreach (var constructorInfo in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						var ctrParams = constructorInfo.GetParameters();
						if (ctrParams.Length != argCount)
							continue;
						if (argCount > 0 && typeof(Arg1T).IsAssignableFrom(ctrParams[0].ParameterType) == false)
							continue;
						if (argCount > 1 && typeof(Arg2T).IsAssignableFrom(ctrParams[1].ParameterType) == false)
							continue;
						if (argCount > 2 && typeof(Arg3T).IsAssignableFrom(ctrParams[2].ParameterType) == false)
							continue;

						foundCtr = constructorInfo;
						break;
					}

					if (foundCtr != null)
					{
						var arg1Param = Expression.Parameter(typeof(Arg1T), "arg1");
						var arg2Param = Expression.Parameter(typeof(Arg2T), "arg2");
						var arg3Param = Expression.Parameter(typeof(Arg3T), "arg3");
						switch (argCount)
						{
							case 1:
								ctr = Expression.Lambda<Func<Arg1T, object>>
									(
										Expression.Convert(
											Expression.New(foundCtr, arg1Param),
											typeof(object)
											),
										arg1Param
									).Compile();
								break;
							case 2:
								ctr = Expression.Lambda<Func<Arg1T, Arg2T, object>>
									(
										Expression.Convert(
											Expression.New(foundCtr, arg1Param, arg2Param),
											typeof(object)
											),
										arg1Param,
										arg2Param
									).Compile();
								break;
							case 3:
								ctr = Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, object>>
									(
										Expression.Convert(
											Expression.New(foundCtr, arg1Param, arg2Param, arg3Param),
											typeof(object)
											),
										arg1Param,
										arg2Param,
										arg3Param
									).Compile();
								break;
						}
					}

					CustomConstructorCache[signature] = ctr;
				}
			}

			var instance = default(object);
			// create instance
			switch (argCount)
			{
				case 1:
					if (ctr == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1})'.", type, typeof(Arg1T).Name), "type");
					instance = ((Func<Arg1T, object>)ctr)(arg1);
					break;
				case 2:
					if (ctr == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1}, {2})'.", type, typeof(Arg1T).Name, typeof(Arg2T).Name), "type");
					instance = ((Func<Arg1T, Arg2T, object>)ctr)(arg1, arg2);
					break;
				case 3:
					if (ctr == null)
						throw new ArgumentException(string.Format("Type '{0}' does not contains constructor with signature 'ctr({1}, {2}, {3})'.", type, typeof(Arg1T).Name, typeof(Arg2T).Name, typeof(Arg3T).Name), "type");
					instance = ((Func<Arg1T, Arg2T, Arg3T, object>)ctr)(arg1, arg2, arg3);
					break;
			}

			return instance;
		}
	}
}
