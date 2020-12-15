using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Conversion metadata provider used by <see cref="ITypeConversionProvider"/> to discover conversion methods on types. 
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public interface IConversionMetadataProvider
	{
		/// <summary>
		/// Get a list of conversion methods from <paramref name="type"/> type.  These method will accept instance of <paramref name="type"/> or declared on <paramref name="type"/>.
		/// </summary>
		/// <returns>List of conversion methods.</returns>
		[NotNull, ItemNotNull, MustUseReturnValue]
		IReadOnlyCollection<ConversionMethodInfo> GetConvertFromMethods([NotNull] Type type);

		/// <summary>
		/// Get a list of conversion methods to <paramref name="type"/> type. These method will return instance of <paramref name="type"/> or are constructors.
		/// </summary>
		/// <returns>List of conversion methods.</returns>
		[NotNull, ItemNotNull, MustUseReturnValue]
		IReadOnlyCollection<ConversionMethodInfo> GetConvertToMethods([NotNull] Type type);
#if !NETSTANDARD
		/// <summary>
		/// Get <see cref="System.ComponentModel.TypeConverter"/> instance associated with type.
		/// </summary>
		/// <param name="type">Type to look <see cref="System.ComponentModel.TypeConverter"/> on.</param>
		/// <returns>Instance of <see cref="System.ComponentModel.TypeConverter"/> or null if no <see cref="System.ComponentModel.TypeConverter"/> defined on type.</returns>
		[CanBeNull, MustUseReturnValue]
		System.ComponentModel.TypeConverter GetTypeConverter([NotNull] Type type);
#endif
		/// <summary>
		/// Checks if <paramref name="type"/> is <paramref name="fromType"/> or has <paramref name="fromType"/> in base types or in interfaces.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="fromType">Base type or interface to find.</param>
		/// <returns>True if found.</returns>
		[MustUseReturnValue]
		bool IsAssignableFrom([NotNull]Type type, [NotNull]Type fromType);

		/// <summary>
		/// Check if passed parameter is 'format' parameter and has <see cref="string"/> type.
		/// </summary>
		/// <param name="methodParameter">Parameter to check.</param>
		/// <returns>True if parameter is 'format' parameter.</returns>
		[MustUseReturnValue]
		bool IsFormatParameter([NotNull] ParameterInfo methodParameter);
		/// <summary>
		/// Check if passed parameter is 'formatProvider' parameter and has <see cref="IFormatProvider"/> type.
		/// </summary>
		/// <param name="methodParameter">Parameter to check.</param>
		/// <returns>True if parameter is 'formatProvider' parameter.</returns>
		[MustUseReturnValue]
		bool IsFormatProviderParameter([NotNull] ParameterInfo methodParameter);
		/// <summary>
		/// Get default value for 'format' parameter for specified conversion. Typically default value of 'format' parameter is taken.
		/// </summary>
		/// <param name="conversionMethodInfo">Conversion method to look on.</param>
		/// <returns>Format value or null if not found.</returns>
		[CanBeNull, MustUseReturnValue]
		string GetDefaultFormat([NotNull] ConversionMethodInfo conversionMethodInfo);
	}
}
