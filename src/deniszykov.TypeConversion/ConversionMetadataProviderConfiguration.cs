using System;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Configuration for <see cref="ConversionMetadataProvider"/>
	/// </summary>
	[DataContract]
	public class ConversionMetadataProviderConfiguration
	{
		/// <summary>
		/// List of method names used to create value of some type from other type. If not set then default list of names used. Example: 'Parse', 'Create', 'From'.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public string[] ConvertFromMethodNames;
		/// <summary>
		/// List of method names used to transform value of some type to other type. If not set then default list of names used. Example: 'ToString', 'ToNumber'.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public string[] ConvertToMethodNames;
		/// <summary>
		/// List of possible names for '<see cref="string"/> format' parameter. Default value is 'format'.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public string[] FormatParameterNames;
		/// <summary>
		/// List of possible names for '<see cref="IFormatProvider"/> formatProvider' parameter. If not set then format provider parameter determined by it's type. Default value is null.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public string[] FormatProviderParameterNames;

		/// <summary>
		/// List of additional methods used to convert between types. Keep this list short because it's scanned each time for each new converted type.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public MethodInfo[] AdditionalConversionMethods;
		/// <summary>
		/// List of methods which should not be used to to convert between types.  Keep this list short because it's scanned each time for each new converted type.
		/// </summary>
		[DataMember, CanBeNull, ItemNotNull]
		public MethodBase[] ForbiddenConversionMethods;
		/// <summary>
		/// Predicate used to filter undesired conversion methods during method discovery.
		/// </summary>
		[CanBeNull]
		public Func<MethodBase, bool> MethodFilter;
	}
}
