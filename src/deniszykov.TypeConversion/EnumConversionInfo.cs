/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
// ReSharper disable once RedundantUsingDirective Used in .NET Standard target
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

#pragma warning disable CS8714, CS8604 // possible nulls, but enum values are never nulls

// ReSharper disable RedundantCast
namespace deniszykov.TypeConversion
{
	/// <summary>
	/// Provides methods of conversion between <see cref="Enum"/> type and it's underlying type.
	/// </summary>
	/// <typeparam name="EnumT">Enum type.</typeparam>
	[PublicAPI]
	public class EnumConversionInfo<EnumT> : IEnumConversionInfo
	{
		private readonly SortedDictionary<EnumT, string> namesByValue;
		private readonly SortedDictionary<string, EnumT> valueByName;
		private readonly SortedDictionary<string, EnumT> valueByNameCaseInsensitive;

		/// <inheritdoc />
		public Type Type { get; }
		// ReSharper disable StaticMemberInGenericType
		/// <summary>
		/// <typeparamref name="EnumT"/> to Number(SByte,Byte,Int16...) conversion function. Instance of <see cref="Func{T1, TResult}"/> where T1 is <typeparamref name="EnumT"/> and TResult is number type.
		/// </summary>
		public Delegate ToNumber { get; }
		/// <summary>
		///  Number(SByte,Byte,Int16...) to <typeparamref name="EnumT"/> conversion function. Instance of <see cref="Func{T1, TResult}"/> where T1 is number type and TResult is <typeparamref name="EnumT"/> type.
		/// </summary>
		public Delegate FromNumber { get; set; }
		/// <summary>
		/// Comparer for <typeparamref name="EnumT"/> values.
		/// </summary>
		public Comparer<EnumT> Comparer { get; }
		/// <summary>
		/// Type code of enum underlying type.
		/// </summary>
		public TypeCode UnderlyingTypeCode { get; }
		/// <summary>
		/// Type of enum underlying type.
		/// </summary>
		public Type UnderlyingType { get; }
		/// <summary>
		/// Flag indicating what enum has <see cref="FlagsAttribute"/>.
		/// </summary>
		public bool IsFlags { get; }
		/// <summary>
		/// Flag indicating what Enum <see cref="UnderlyingType"/> is signed number.
		/// </summary>
		public bool IsSigned { get; }
		/// <summary>
		/// Maximum value for enumeration (declared values only).
		/// </summary>
		public EnumT MaxValue { get; }
		/// <summary>
		/// Default value for enumeration (zero).
		/// </summary>
		public EnumT DefaultValue { get; }
		/// <summary>
		/// Minimum value for enumeration (declared values only).
		/// </summary>
		public EnumT MinValue { get; }
		/// <summary>
		/// Names of all enumeration values. Order is corresponding to <see cref="Values"/>.
		/// </summary>
		public ReadOnlyCollection<string> Names { get; }
		/// <summary>
		/// All declared enumeration values. Order is corresponding to <see cref="Names"/>. 
		/// </summary>
		public ReadOnlyCollection<EnumT> Values { get; }
		// ReSharper restore StaticMemberInGenericType

		public EnumConversionInfo(bool useDynamicMethods)
		{
			var enumType = typeof(EnumT);
#if NETSTANDARD
			var enumTypeInfo = enumType.GetTypeInfo();
#else
			var enumTypeInfo = enumType;
#endif
			if (enumTypeInfo.IsEnum == false)
				throw new InvalidOperationException($"{typeof(EnumT).FullName} should be enum type.");

			var underlyingType = Enum.GetUnderlyingType(enumType)!;

			this.Type = enumType;
			this.UnderlyingType = underlyingType;
			this.UnderlyingTypeCode = Convert.GetTypeCode(Activator.CreateInstance(enumType));
			this.IsFlags = enumTypeInfo.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
			this.IsSigned = this.UnderlyingTypeCode == TypeCode.SByte || this.UnderlyingTypeCode == TypeCode.Int16 || this.UnderlyingTypeCode == TypeCode.Int32 || this.UnderlyingTypeCode == TypeCode.Int64;

			if (!useDynamicMethods)
			{
				this.FromNumber = GetFromNumberAot(this.UnderlyingTypeCode);
				this.ToNumber = GetToNumberAot(this.UnderlyingTypeCode);
				this.Comparer = Comparer<EnumT>.Default;
			}
			else
			{
				var valueParameter = Expression.Parameter(underlyingType, "value");
				var enumParameter = Expression.Parameter(enumType, "value");
				var xParameter = Expression.Parameter(enumType, "value");
				var yParameter = Expression.Parameter(enumType, "value");

				this.FromNumber = Expression.Lambda(Expression.ConvertChecked(valueParameter, enumType), valueParameter).Compile();
				this.ToNumber = Expression.Lambda(Expression.ConvertChecked(enumParameter, underlyingType), enumParameter).Compile();
				this.Comparer = new ComparisonComparer<EnumT>(Expression.Lambda<Comparison<EnumT>>(
					Expression.Call
					(
						Expression.ConvertChecked(xParameter, underlyingType),
						"CompareTo",
						Type.EmptyTypes,
						Expression.ConvertChecked(yParameter, underlyingType)
					),
					xParameter,
					yParameter
				).Compile());
			}

			this.namesByValue = new SortedDictionary<EnumT, string>(this.Comparer);
			this.valueByName = new SortedDictionary<string, EnumT>(StringComparer.Ordinal);
			this.valueByNameCaseInsensitive = new SortedDictionary<string, EnumT>(StringComparer.OrdinalIgnoreCase);
			this.DefaultValue = this.MinValue = this.MaxValue = default(EnumT)!;

			var valuesArray = (EnumT[])Enum.GetValues(enumType)!;
			var names = new List<string>(valuesArray.Length);
			var values = new List<EnumT>(valuesArray.Length);
			foreach (var value in valuesArray)
			{
				var name = Enum.GetName(enumType, value);
				if (string.IsNullOrEmpty(name))
					continue;

				this.namesByValue[value] = name;
				this.valueByName[name] = value;
				this.valueByNameCaseInsensitive[name] = value;
				names.Add(name);
				values.Add(value);

				this.MinValue = this.Comparer.Compare(value, this.MinValue) < 0 ? value : this.MinValue;
				this.MaxValue = this.Comparer.Compare(value, this.MaxValue) > 0 ? value : this.MaxValue;
			}

			if (values.Contains(this.DefaultValue) == false)
			{
				// if default value is not part of Values then
				// swap MinValue and MaxValue and re-calculate them
				var tempMaxValue = this.MaxValue;
				this.MaxValue = this.MinValue;
				this.MinValue = tempMaxValue;

				foreach (var value in values)
				{
					this.MinValue = this.Comparer.Compare(value, this.MinValue) < 0 ? value : this.MinValue;
					this.MaxValue = this.Comparer.Compare(value, this.MaxValue) > 0 ? value : this.MaxValue;
				}
			}

			this.Values = values.AsReadOnly();
			this.Names = names.AsReadOnly();
		}

		/// <summary>
		/// Get name of passed enumeration value.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns>Cached name of enumeration member or string representation of it's underlying type.</returns>
		public string ToName(EnumT value)
		{
			if (this.namesByValue.TryGetValue(value, out var name))
				return name;

			return Convert.ToString(value)!;
		}

		/// <summary>
		/// Convert passed enumeration value to <see cref="SByte"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="SByte"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="SByte"/> value of passed enumeration value.</returns>
		public sbyte ToSByte(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (sbyte)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (sbyte)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (sbyte)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (sbyte)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (sbyte)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (sbyte)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (sbyte)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (sbyte)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (sbyte)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (sbyte)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (sbyte)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (sbyte)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (sbyte)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (sbyte)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (sbyte)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (sbyte)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Byte"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Byte"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Byte"/> value of passed enumeration value.</returns>
		public byte ToByte(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (byte)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (byte)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (byte)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (byte)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (byte)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (byte)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (byte)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (byte)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (byte)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (byte)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (byte)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (byte)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (byte)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (byte)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (byte)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (byte)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int16"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int16"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Int16"/> value of passed enumeration value.</returns>
		public short ToInt16(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (short)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (short)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (short)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (short)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (short)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (short)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (short)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (short)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (short)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (short)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (short)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (short)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (short)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (short)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (short)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (short)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt16"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt16"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="UInt16"/> value of passed enumeration value.</returns>
		public ushort ToUInt16(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (ushort)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (ushort)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (ushort)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (ushort)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (ushort)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (ushort)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (ushort)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (ushort)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (ushort)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (ushort)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (ushort)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (ushort)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (ushort)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (ushort)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (ushort)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (ushort)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int32"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int32"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Int32"/> value of passed enumeration value.</returns>
		public int ToInt32(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (int)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (int)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (int)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (int)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (int)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (int)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (int)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (int)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (int)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (int)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (int)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (int)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (int)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (int)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (int)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (int)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt32"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt32"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="UInt32"/> value of passed enumeration value.</returns>
		public uint ToUInt32(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (uint)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (uint)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (uint)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (uint)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (uint)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (uint)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (uint)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (uint)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (uint)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (uint)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (uint)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (uint)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (uint)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (uint)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (uint)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (uint)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int64"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int64"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Int64"/> value of passed enumeration value.</returns>
		public long ToInt64(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (long)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (long)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (long)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (long)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (long)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (long)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (long)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (long)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (long)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (long)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (long)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (long)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (long)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (long)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (long)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (long)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt64"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt64"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="UInt64"/> value of passed enumeration value.</returns>
		public ulong ToUInt64(EnumT value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (ulong)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (ulong)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (ulong)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (ulong)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (ulong)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (ulong)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (ulong)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (ulong)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return (ulong)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
						case TypeCode.Byte: return (ulong)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
						case TypeCode.Int16: return (ulong)((Func<EnumT, short>)this.ToNumber).Invoke(value);
						case TypeCode.UInt16: return (ulong)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
						case TypeCode.Int32: return (ulong)((Func<EnumT, int>)this.ToNumber).Invoke(value);
						case TypeCode.UInt32: return (ulong)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
						case TypeCode.Int64: return (ulong)((Func<EnumT, long>)this.ToNumber).Invoke(value);
						case TypeCode.UInt64: return (ulong)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Single"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Single"/> value of passed enumeration value.</returns>
		public float ToSingle(EnumT value)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (this.UnderlyingTypeCode)
			{
				case TypeCode.SByte: return (float)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
				case TypeCode.Byte: return (float)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
				case TypeCode.Int16: return (float)((Func<EnumT, short>)this.ToNumber).Invoke(value);
				case TypeCode.UInt16: return (float)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
				case TypeCode.Int32: return (float)((Func<EnumT, int>)this.ToNumber).Invoke(value);
				case TypeCode.UInt32: return (float)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
				case TypeCode.Int64: return (float)((Func<EnumT, long>)this.ToNumber).Invoke(value);
				case TypeCode.UInt64: return (float)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Double"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Double"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns><see cref="Double"/> value of passed enumeration value.</returns>
		public double ToDouble(EnumT value)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (this.UnderlyingTypeCode)
			{
				case TypeCode.SByte: return (double)((Func<EnumT, sbyte>)this.ToNumber).Invoke(value);
				case TypeCode.Byte: return (double)((Func<EnumT, byte>)this.ToNumber).Invoke(value);
				case TypeCode.Int16: return (double)((Func<EnumT, short>)this.ToNumber).Invoke(value);
				case TypeCode.UInt16: return (double)((Func<EnumT, ushort>)this.ToNumber).Invoke(value);
				case TypeCode.Int32: return (double)((Func<EnumT, int>)this.ToNumber).Invoke(value);
				case TypeCode.UInt32: return (double)((Func<EnumT, uint>)this.ToNumber).Invoke(value);
				case TypeCode.Int64: return (double)((Func<EnumT, long>)this.ToNumber).Invoke(value);
				case TypeCode.UInt64: return (double)((Func<EnumT, ulong>)this.ToNumber).Invoke(value);
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}
		}

		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromSByte(sbyte value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromByte(byte value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromInt16(short value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration . Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromUInt16(ushort value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromInt32(int value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromUInt32(uint value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromInt64(long value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromUInt64(ulong value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromSingle(float value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <param name="checkedConversion">Use checked conversion and allow/disallow overflow.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public EnumT FromDouble(double value, bool checkedConversion = true)
		{
			if (checkedConversion)
			{
				checked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
			else
			{
				unchecked
				{
					// ReSharper disable once SwitchStatementMissingSomeCases
					switch (this.UnderlyingTypeCode)
					{
						case TypeCode.SByte: return ((Func<sbyte, EnumT>)this.FromNumber).Invoke((sbyte)value);
						case TypeCode.Byte: return ((Func<byte, EnumT>)this.FromNumber).Invoke((byte)value);
						case TypeCode.Int16: return ((Func<short, EnumT>)this.FromNumber).Invoke((short)value);
						case TypeCode.UInt16: return ((Func<ushort, EnumT>)this.FromNumber).Invoke((ushort)value);
						case TypeCode.Int32: return ((Func<int, EnumT>)this.FromNumber).Invoke((int)value);
						case TypeCode.UInt32: return ((Func<uint, EnumT>)this.FromNumber).Invoke((uint)value);
						case TypeCode.Int64: return ((Func<long, EnumT>)this.FromNumber).Invoke((long)value);
						case TypeCode.UInt64: return ((Func<ulong, EnumT>)this.FromNumber).Invoke((ulong)value);
						default:
							throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
					}
				}
			}
		}

		/// <summary>
		/// Check if passed enumeration value is defined in enumeration.
		/// </summary>
		/// <param name="value">Enumeration value to check.</param>
		/// <returns>True if defined. False if not defined.</returns>
		public bool IsDefined(EnumT value)
		{
			if (this.IsFlags)
			{
				return Enum.IsDefined(typeof(EnumT), value);
			}
			else
			{
				return this.namesByValue.ContainsKey(value);
			}
		}
		/// <summary>
		/// Map passed <paramref name="name"/> to enumeration member name and return it's value. Or parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <returns>Enumeration value.</returns>
		public EnumT Parse(string name)
		{
			// ReSharper disable once IntroduceOptionalParameters.Global
			return this.Parse(name, ignoreCase: false);
		}
		/// <summary>
		/// Map passed <paramref name="name"/> to enumeration member name and return it's value. Or parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <param name="ignoreCase">Ignore case of enumerable names during parsing.</param>
		/// <returns>Enumeration value.</returns>
		public EnumT Parse(string name, bool ignoreCase)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var byNameMap = ignoreCase ? this.valueByNameCaseInsensitive : this.valueByName;
			if (byNameMap.TryGetValue(name, out var value))
				return value;

			if (this.TryParseNumber(name, out value))
				return value;

			value = (EnumT)Enum.Parse(typeof(EnumT), name, ignoreCase);
			return value;
		}
		/// <summary>
		/// Try to map passed <paramref name="name"/> to enumeration member name and return it's value. Or try to parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <param name="value">Mapped enumeration value.</param>
		/// <returns>True if mapped successfully. False if mapping failed.</returns>
		public bool TryParse(string name, out EnumT value)
		{
			return this.TryParse(name, out value, ignoreCase: false);
		}
		/// <summary>
		/// Try to map passed <paramref name="name"/> to enumeration member name and return it's value. Or try to parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <param name="value">Mapped enumeration value.</param>
		/// <param name="ignoreCase">Ignore case of enumerable names during parsing.</param>
		/// <returns>True if mapped successfully. False if mapping failed.</returns>
		public bool TryParse(string name, out EnumT value, bool ignoreCase)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			value = default(EnumT)!;

			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			var byNameMap = ignoreCase ? this.valueByNameCaseInsensitive : this.valueByName;
			if (byNameMap.TryGetValue(name, out value))
				return true;

			try
			{
				if (this.TryParseNumber(name, out value))
					return true;

				value = (EnumT)Enum.Parse(typeof(EnumT), name, ignoreCase);
				return true;
			}
			catch (OverflowException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		private bool TryParseNumber(string number, out EnumT value)
		{
			if (number == null) throw new ArgumentNullException(nameof(number));

			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (this.UnderlyingTypeCode)
			{
				case TypeCode.SByte:
					if (sbyte.TryParse(number, out var int8Value))
					{
						value = ((Func<sbyte, EnumT>)this.FromNumber).Invoke(int8Value);
						return true;
					}
					break;
				case TypeCode.Byte:
					if (byte.TryParse(number, out var uint8Value))
					{
						value = ((Func<byte, EnumT>)this.FromNumber).Invoke(uint8Value);
						return true;
					}
					break;
				case TypeCode.Int16:
					if (short.TryParse(number, out var int16Value))
					{
						value = ((Func<short, EnumT>)this.FromNumber).Invoke(int16Value);
						return true;
					}
					break;
				case TypeCode.UInt16:
					if (ushort.TryParse(number, out var uint16Value))
					{
						value = ((Func<ushort, EnumT>)this.FromNumber).Invoke(uint16Value);
						return true;
					}
					break;
				case TypeCode.Int32:
					if (int.TryParse(number, out var int32Value))
					{
						value = ((Func<int, EnumT>)this.FromNumber).Invoke(int32Value);
						return true;
					}
					break;
				case TypeCode.UInt32:
					if (uint.TryParse(number, out var uint32Value))
					{
						value = ((Func<uint, EnumT>)this.FromNumber).Invoke(uint32Value);
						return true;
					}
					break;
				case TypeCode.Int64:
					if (long.TryParse(number, out var int64Value))
					{
						value = ((Func<long, EnumT>)this.FromNumber).Invoke(int64Value);
						return true;
					}
					break;
				case TypeCode.UInt64:
					if (ulong.TryParse(number, out var uint64Value))
					{
						value = ((Func<ulong, EnumT>)this.FromNumber).Invoke(uint64Value);
						return true;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{this.UnderlyingTypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}

			value = default(EnumT)!;
			return false;
		}

		private static Delegate GetToNumberAot(TypeCode underlyingTypeCode)
		{
			switch (underlyingTypeCode)
			{
				case TypeCode.SByte:
					return new Func<EnumT, sbyte>(value => (sbyte)(object)value);
				case TypeCode.Byte:
					return new Func<EnumT, byte>(value => (byte)(object)value);
				case TypeCode.Int16:
					return new Func<EnumT, short>(value => (short)(object)value);
				case TypeCode.UInt16:
					return new Func<EnumT, ushort>(value => (ushort)(object)value);
				case TypeCode.Int32:
					return new Func<EnumT, int>(value => (int)(object)value);
				case TypeCode.UInt32:
					return new Func<EnumT, uint>(value => (uint)(object)value);
				case TypeCode.Int64:
					return new Func<EnumT, long>(value => (long)(object)value);
				case TypeCode.UInt64:
					return new Func<EnumT, ulong>(value => (ulong)(object)value);
				case TypeCode.Object:
				case TypeCode.Empty:
				case TypeCode.Boolean:
#if !NETSTANDARD1_6
				case TypeCode.DBNull:
#endif
				case TypeCode.Char:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.DateTime:
				case TypeCode.String:
				default:
					throw new ArgumentOutOfRangeException(nameof(underlyingTypeCode), underlyingTypeCode, null);
			}
		}
		private static Delegate GetFromNumberAot(TypeCode underlyingTypeCode)
		{
			switch (underlyingTypeCode)
			{
				case TypeCode.SByte:
					return new Func<sbyte, EnumT>(value => (EnumT)(object)value);
				case TypeCode.Byte:
					return new Func<byte, EnumT>(value => (EnumT)(object)value);
				case TypeCode.Int16:
					return new Func<short, EnumT>(value => (EnumT)(object)value);
				case TypeCode.UInt16:
					return new Func<ushort, EnumT>(value => (EnumT)(object)value);
				case TypeCode.Int32:
					return new Func<int, EnumT>(value => (EnumT)(object)value);
				case TypeCode.UInt32:
					return new Func<uint, EnumT>(value => (EnumT)(object)value);
				case TypeCode.Int64:
					return new Func<long, EnumT>(value => (EnumT)(object)value);
				case TypeCode.UInt64:
					return new Func<ulong, EnumT>(value => (EnumT)(object)value);
				case TypeCode.Object:
				case TypeCode.Empty:
				case TypeCode.Boolean:
#if !NETSTANDARD1_6
				case TypeCode.DBNull:
#endif
				case TypeCode.Char:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.DateTime:
				case TypeCode.String:
				default:
					throw new ArgumentOutOfRangeException(nameof(underlyingTypeCode), underlyingTypeCode, null);
			}
		}

		private class ComparisonComparer<T> : Comparer<T>
		{
			private readonly Comparison<T> comparison;

			public ComparisonComparer(Comparison<T> comparison)
			{
				if (comparison == null) throw new ArgumentNullException(nameof(comparison));

				this.comparison = comparison;
			}

			public override int Compare(T x, T y)
			{
				return this.comparison(x, y);
			}
		}
	}
}
