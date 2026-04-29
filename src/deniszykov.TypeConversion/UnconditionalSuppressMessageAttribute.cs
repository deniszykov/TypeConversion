// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

#if !NET5_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// Suppresses reporting of a specific rule violation, allowing multiple suppressions on a
	/// single code artifact.
	/// </summary>
	/// <remarks>
	/// <see cref="UnconditionalSuppressMessageAttribute"/> is different than
	/// <see cref="SuppressMessageAttribute"/> in that it doesn't have a
	/// <see cref="ConditionalAttribute"/>. So it is always preserved in the compiled assembly.
	/// </remarks>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal sealed class UnconditionalSuppressMessageAttribute : Attribute
	{
		public UnconditionalSuppressMessageAttribute(string category, string checkId)
		{
			Category = category;
			CheckId = checkId;
		}

		public string Category { get; }
		public string CheckId { get; }
		public string? Scope { get; set; }
		public string? Target { get; set; }
		public string? MessageId { get; set; }
		public string? Justification { get; set; }
	}
}
#endif
