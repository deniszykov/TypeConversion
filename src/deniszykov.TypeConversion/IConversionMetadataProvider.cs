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
		bool IsAssignableFrom([NotNull]Type type, [NotNull]Type fromType);
		bool IsFormatParameter([NotNull] ParameterInfo methodParameter);
		bool IsFormatProviderParameter([NotNull] ParameterInfo methodParameter);
		[CanBeNull]
		string GetDefaultFormat([NotNull] ConversionMethodInfo conversionMethodInfo);
	}
}
