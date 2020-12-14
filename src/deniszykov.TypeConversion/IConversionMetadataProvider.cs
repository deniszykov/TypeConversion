using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	public interface IConversionMetadataProvider
	{
		[NotNull, ItemNotNull]
		IReadOnlyCollection<ConversionMethodInfo> GetConvertFromMethods([NotNull] Type type);
		[NotNull, ItemNotNull]
		IReadOnlyCollection<ConversionMethodInfo> GetConvertToMethods([NotNull] Type type);
#if !NETSTANDARD
		[CanBeNull]
		System.ComponentModel.TypeConverter GetTypeConverter([NotNull] Type type);
#endif
		/// <summary>
		/// Checks if <paramref name="type"/> is <paramref name="fromType"/> or has <paramref name="fromType"/> in base types or in interfaces.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="fromType">Base type or interface to find.</param>
		/// <returns>True if found.</returns>
		bool IsAssignableFrom([NotNull]Type type, [NotNull]Type fromType);
		bool IsFormatParameter([NotNull] ParameterInfo methodParameter);
		bool IsFormatProviderParameter([NotNull] ParameterInfo methodParameter);
		[CanBeNull]
		string GetDefaultFormat([NotNull] ConversionMethodInfo conversionMethodInfo);
	}
}
