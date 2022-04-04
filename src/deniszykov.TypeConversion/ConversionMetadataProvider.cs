using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Conversion metadata provider used by <see cref="ITypeConversionProvider"/> to discover conversion methods on types. 
	/// </summary>
	[PublicAPI]
	public class ConversionMetadataProvider : IConversionMetadataProvider
	{
		private class ConversionTypeInfo
		{
			private static readonly IReadOnlyCollection<ConversionMethodInfo> EmptyConversionMethods = new ConversionMethodInfo[0];

			private readonly Type type;
			private readonly HashSet<Type> implementsAndExtends;
			public readonly IReadOnlyCollection<ConversionMethodInfo> ConvertFromMethods;
			public readonly IReadOnlyCollection<ConversionMethodInfo> ConvertToMethods;
#if !NETSTANDARD
			public readonly System.ComponentModel.TypeConverter? TypeConverter;
#endif

			public ConversionTypeInfo(ConversionMetadataProvider provider, Type type)
			{
				if (provider == null) throw new ArgumentNullException(nameof(provider));
				if (type == null) throw new ArgumentNullException(nameof(type));

				this.type = type;

				var fromMethods = default(List<ConversionMethodInfo>);
				var toMethods = default(List<ConversionMethodInfo>);

				foreach (var method in ReflectionExtensions.GetPublicMethods(type, declaredOnly: true).Concat(provider.additionalConversionMethods))
				{
					if (provider.IsPossibleConvertMethod(method) == false ||
						provider.forbiddenConversionMethods.Contains(method) ||
						(provider.methodFilter?.Invoke(method) ?? false))
					{
						continue;
					}

					var parameters = method.GetParameters();
					if (!parameters.All(provider.IsPlainParameter))
					{
						continue;  // some ref/out/by ref like/pointer parameters
					}
					if (method.ReturnParameter != null && provider.IsPlainParameter(method.ReturnParameter) == false)
					{
						continue;  // ref/out/by ref like/pointer return type
					}

					// Explicit/Implicit operators
					if (method.IsStatic && method.IsSpecialName && (method.Name == "op_Explicit" || method.Name == "op_Implicit"))
					{
						if (parameters.Length == 1 && parameters[0].ParameterType == type)
						{
							toMethods ??= new List<ConversionMethodInfo>(10);
							toMethods.Add(new ConversionMethodInfo(method, parameters, provider.MapParameters(parameters, type)));
						}
						else if (parameters.Length == 1 && method.ReturnType == type)
						{
							fromMethods ??= new List<ConversionMethodInfo>(10);
							fromMethods.Add(new ConversionMethodInfo(method, parameters, provider.MapParameters(parameters, null)));
						}
					}

					// custom FromX method
					if (provider.IsConvertFromMethod(method, type, parameters, out var fromValueParameter))
					{
						fromMethods ??= new List<ConversionMethodInfo>(10);
						fromMethods.Add(new ConversionMethodInfo(method, parameters, provider.MapParameters(parameters, fromValueParameter?.ParameterType)));
					}

					// custom ToX method
					if (provider.IsConvertToMethod(method, type, parameters, out fromValueParameter))
					{
						toMethods ??= new List<ConversionMethodInfo>(10);
						toMethods.Add(new ConversionMethodInfo(method, parameters, provider.MapParameters(parameters, fromValueParameter?.ParameterType)));
					}
				}

				foreach (var constructor in ReflectionExtensions.GetPublicConstructors(type))
				{
					if (provider.forbiddenConversionMethods.Contains(constructor) ||
						(provider.methodFilter?.Invoke(constructor) ?? false))
					{
						continue;
					}

					var parameters = constructor.GetParameters();

					if (parameters.Length != 1 ||
						provider.IsPlainParameter(parameters[0]) == false ||
						parameters[0].ParameterType == type)
					{
						continue;
					}

					fromMethods ??= new List<ConversionMethodInfo>(10);
					fromMethods.Add(new ConversionMethodInfo(constructor, parameters, provider.MapParameters(parameters, null)));
				}

				this.ConvertFromMethods = fromMethods ?? EmptyConversionMethods;
				this.ConvertToMethods = toMethods ?? EmptyConversionMethods;

				this.implementsAndExtends = new HashSet<Type>(ReflectionExtensions.EnumerateBaseTypesAndInterfaces(type));

#if !NETSTANDARD
				this.TypeConverter = System.ComponentModel.TypeDescriptor.GetConverter(type);
				if (this.TypeConverter.GetType() == typeof(System.ComponentModel.TypeConverter))
					this.TypeConverter = null;
#endif
			}

			public bool IsAssignableFrom(Type type)
			{
				if (type == null) throw new ArgumentNullException(nameof(type));

				if (type == typeof(object))
				{
					return true;
				}

				return this.implementsAndExtends.Contains(type);
			}

			/// <inheritdoc />
			public override string ToString() => this.type.ToString();
		}

		private readonly ConcurrentDictionary<Type, ConversionTypeInfo> cachedConversionTypeInfos;
		private readonly Func<Type, ConversionTypeInfo> createConversionTypeInfo;
		private readonly HashSet<string> convertFromMethodNames;
		private readonly HashSet<string> convertToMethodNames;
		private readonly HashSet<string> formatParameterNames;
		private readonly HashSet<string> formatProviderParameterNames;
		private readonly MethodInfo[] additionalConversionMethods;
		private readonly HashSet<MethodBase> forbiddenConversionMethods;
		private readonly Func<MethodBase, bool>? methodFilter;
		/// <summary>
		/// Constructor of <see cref="ConversionMetadataProvider"/>.
		/// </summary>
		public ConversionMetadataProvider(
#if NET45
			 ConversionMetadataProviderConfiguration? configuration = null
#else
			 Microsoft.Extensions.Options.IOptions<ConversionMetadataProviderConfiguration>? configurationOptions = null

#endif
		)
		{
#if !NET45
			var configuration = configurationOptions?.Value;
#endif


			this.cachedConversionTypeInfos = new ConcurrentDictionary<Type, ConversionTypeInfo>();
			this.createConversionTypeInfo = type => new ConversionTypeInfo(this, type);
			this.convertFromMethodNames = new HashSet<string>(configuration?.ConvertFromMethodNames ?? new[] { "Parse", "Create" }, StringComparer.OrdinalIgnoreCase);
			this.convertToMethodNames = new HashSet<string>(configuration?.ConvertToMethodNames ?? new[] { "ToString" }, StringComparer.OrdinalIgnoreCase);
			this.formatParameterNames = new HashSet<string>(configuration?.FormatParameterNames ?? new[] { "format" });
			this.formatProviderParameterNames = new HashSet<string>(configuration?.FormatProviderParameterNames ?? new string[0]);
			this.additionalConversionMethods = configuration?.AdditionalConversionMethods ?? new MethodInfo[0];
			this.forbiddenConversionMethods = new HashSet<MethodBase>(configuration?.ForbiddenConversionMethods ?? new MethodBase[0]);
			this.methodFilter = configuration?.MethodFilter;
		}
		/// <inheritdoc />
		public IReadOnlyCollection<ConversionMethodInfo> GetConvertFromMethods(Type type)
		{
			var conversionTypeInfo = this.cachedConversionTypeInfos.GetOrAdd(type, this.createConversionTypeInfo);
			return conversionTypeInfo.ConvertFromMethods;
		}
		/// <inheritdoc />
		public IReadOnlyCollection<ConversionMethodInfo> GetConvertToMethods(Type type)
		{
			var conversionTypeInfo = this.cachedConversionTypeInfos.GetOrAdd(type, this.createConversionTypeInfo);
			return conversionTypeInfo.ConvertToMethods;
		}
#if !NETSTANDARD
		/// <inheritdoc />
		public System.ComponentModel.TypeConverter? GetTypeConverter(Type type)
		{
			var conversionTypeInfo = this.cachedConversionTypeInfos.GetOrAdd(type, this.createConversionTypeInfo);
			return conversionTypeInfo.TypeConverter;
		}
#endif
		/// <inheritdoc />
		public bool IsAssignableFrom(Type type, Type fromType)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (fromType == null) throw new ArgumentNullException(nameof(fromType));

			var conversionTypeInfo = this.cachedConversionTypeInfos.GetOrAdd(type, this.createConversionTypeInfo);
			return conversionTypeInfo.IsAssignableFrom(fromType);
		}
		/// <inheritdoc />
		public bool IsFormatParameter(ParameterInfo methodParameter)
		{
			if (methodParameter == null) throw new ArgumentNullException(nameof(methodParameter));

			return this.formatParameterNames.Contains(methodParameter.Name ?? "") &&
				methodParameter.ParameterType == typeof(string) &&
				(methodParameter.Attributes & (ParameterAttributes.In | ParameterAttributes.Out | ParameterAttributes.Retval)) == 0;
		}
		/// <inheritdoc />
		public bool IsFormatProviderParameter(ParameterInfo methodParameter)
		{
			if (methodParameter == null) throw new ArgumentNullException(nameof(methodParameter));

			return (this.formatProviderParameterNames.Count == 0 || this.formatProviderParameterNames.Contains(methodParameter.Name ?? "")) &&
				methodParameter.ParameterType == typeof(IFormatProvider) &&
				(methodParameter.Attributes & (ParameterAttributes.In | ParameterAttributes.Out | ParameterAttributes.Retval)) == 0;
		}
		/// <inheritdoc />
		public string? GetDefaultFormat(ConversionMethodInfo conversionMethodInfo)
		{
			if (conversionMethodInfo == null) throw new ArgumentNullException(nameof(conversionMethodInfo));

			foreach (var parameter in conversionMethodInfo.Parameters)
			{
				if (this.IsFormatParameter(parameter) && parameter.HasDefaultValue)
				{
					return parameter.DefaultValue as string;
				}
			}

			return null;
		}

		private bool IsPossibleConvertMethod(MethodInfo method)
		{
			var name = method.Name;
			return string.Equals(name, "op_Explicit", StringComparison.Ordinal) ||
				string.Equals(name, "op_Implicit", StringComparison.Ordinal) ||
				name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
				name.StartsWith("From", StringComparison.OrdinalIgnoreCase) ||
				name.StartsWith("To", StringComparison.OrdinalIgnoreCase) ||
				this.convertFromMethodNames.Contains(name) ||
				this.convertToMethodNames.Contains(name);

		}
		private bool IsConvertFromMethod(MethodInfo method, Type resultType, ParameterInfo[] parameters, out ParameterInfo? fromValueParameter)
		{
			if (method == null) throw new ArgumentNullException(nameof(method));
			if (resultType == null) throw new ArgumentNullException(nameof(resultType));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			fromValueParameter = parameters.FirstOrDefault(p => !this.IsFormatParameter(p) && !this.IsFormatProviderParameter(p));
			if (fromValueParameter == null)
			{
				return false;
			}

			return
				this.IsValidConversionParameters(parameters, fromValueParameter) &&
				method.IsStatic &&
				(method.Name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
					method.Name.StartsWith("From", StringComparison.OrdinalIgnoreCase) ||
					this.convertFromMethodNames.Contains(method.Name)) &&
				method.ReturnType == resultType &&
				method.DeclaringType == resultType;
		}
		private bool IsConvertToMethod(MethodInfo method, Type sourceType, ParameterInfo[] parameters, out ParameterInfo? fromValueParameter)
		{
			if (method == null) throw new ArgumentNullException(nameof(method));
			if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			fromValueParameter = parameters.FirstOrDefault(p => !this.IsFormatParameter(p) && !this.IsFormatProviderParameter(p));
			if (fromValueParameter == null && method.IsStatic)
			{
				return false;
			}

			return this.IsValidConversionParameters(parameters, method.IsStatic ? fromValueParameter : null) &&
				(method.Name.StartsWith("To", StringComparison.OrdinalIgnoreCase) ||
					this.convertToMethodNames.Contains(method.Name)) &&
				method.DeclaringType == sourceType;
		}
		private bool IsValidConversionParameters(ParameterInfo[] parameters, ParameterInfo? fromValueParameter)
		{
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			var param1Index = 0;
			var param2Index = 1;
			var skipParameters = 0;
			if (fromValueParameter != null)
			{
				skipParameters = 1;
				switch (Array.IndexOf(parameters, fromValueParameter))
				{
					case 0:
						param1Index++;
						param2Index++;
						break;
					case 1:
						param2Index++;
						break;
				}
			}

			switch (parameters.Length - skipParameters)
			{
				case 0:
					return true;
				case 1:
					return this.IsFormatParameter(parameters[param1Index]) || this.IsFormatProviderParameter(parameters[param1Index]);
				case 2:
					return this.IsFormatParameter(parameters[param1Index]) ^ this.IsFormatParameter(parameters[param2Index]) &&
						this.IsFormatProviderParameter(parameters[param1Index]) ^ this.IsFormatProviderParameter(parameters[param2Index]);
				default:
					return false;
			}
		}
		private bool IsPlainParameter(ParameterInfo parameterInfo)
		{
			if (parameterInfo == null) throw new ArgumentNullException(nameof(parameterInfo));

			if (IsByRefLike(parameterInfo))
			{
				return false;
			}

			return parameterInfo.IsIn == false && parameterInfo.IsOut == false &&
				parameterInfo.ParameterType.IsByRef == false &&
				parameterInfo.ParameterType.IsPointer == false &&
				parameterInfo.ParameterType.IsGenericParameter == false;
		}

		private ConversionParameterType[] MapParameters(ParameterInfo[] parameters, Type? valueType)
		{
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			var parameterTypes = new ConversionParameterType[parameters.Length];
			for (var p = 0; p < parameters.Length; p++)
			{
				parameterTypes[p] = this.IsFormatParameter(parameters[p]) ? ConversionParameterType.Format :
					this.IsFormatProviderParameter(parameters[p]) ? ConversionParameterType.FormatProvider :
					valueType == null || parameters[p].ParameterType == valueType ? ConversionParameterType.Value :
					throw new InvalidOperationException("Unknown parameter in conversion method.");

			}

			return parameterTypes;
		}

		private static bool IsByRefLike(ParameterInfo parameterInfo)
		{
			if (parameterInfo == null) throw new ArgumentNullException(nameof(parameterInfo));

#if NETCOREAPP
			if (parameterInfo.ParameterType.IsByRefLike)
			{
				return true;
			}
#endif

			foreach (var customAttribute in parameterInfo.GetCustomAttributes(inherit: true))
			{
				var customAttributeType = customAttribute.GetType();
				if (string.Equals(customAttributeType.Name, "IsByRefLikeAttribute", StringComparison.Ordinal) &&
					string.Equals(customAttributeType.Namespace, "System.Runtime.CompilerServices", StringComparison.Ordinal))
				{
					return true;
				}
			}

			return false;
		}

	}
}
