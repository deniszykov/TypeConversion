﻿using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Class providing <see cref="IConverter"/> and <see cref="IConverter{FromType,ToType}"/> instances on demand.
	/// </summary>
	[PublicAPI]
	public interface ITypeConversionProvider
	{
		/// <summary>
		/// Get converter instance which could convert values from <typeparamref name="FromTypeT"/> to <typeparamref name="ToTypeT"/>.
		/// </summary>
		/// <returns>Instance of <see cref="IConverter{FromType,ToType}"/>. Same instance is returned for same generic parameters.</returns>
		[MustUseReturnValue]
		IConverter<FromTypeT, ToTypeT> GetConverter<
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
			FromTypeT, 
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
			ToTypeT
		>();

		/// <summary>
		/// Get converter instance which could convert values from <paramref name="fromType"/> to <paramref name="toType"/>.
		/// </summary>
		/// <returns>Instance of <see cref="IConverter"/>. Same instance is returned for same parameters.</returns>
		[MustUseReturnValue]
		IConverter GetConverter(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
			Type fromType, 
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
			Type toType
		);
	}
}