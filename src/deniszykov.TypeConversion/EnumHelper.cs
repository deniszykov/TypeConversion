/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable RedundantCast
// ReSharper disable once CheckNamespace
namespace System
{
	/// <summary>
	/// Utility class for <see cref="Enum"/> types manipulations.
	/// </summary>
	/// <typeparam name="EnumT"></typeparam>
	public static class EnumHelper<EnumT>
	{
		private static readonly SortedDictionary<EnumT, string> NamesByValue;
		private static readonly SortedDictionary<string, EnumT> ValueByName;
		private static readonly SortedDictionary<string, EnumT> ValueByNameCaseInsensitive;

		// ReSharper disable StaticMemberInGenericType
		/// <summary>
		/// <typeparamref name="EnumT"/> to Number(SByte,Byte,Int16...) conversion function. Instance of <see cref="Func{T1, TResult}"/> where T1 is <typeparamref name="EnumT"/> and TResult is number type.
		/// </summary>
		public static readonly Delegate ToNumber;
		/// <summary>
		///  Number(SByte,Byte,Int16...) to <typeparamref name="EnumT"/> conversion function. Instance of <see cref="Func{T1, TResult}"/> where T1 is number type and TResult is <typeparamref name="EnumT"/> type.
		/// </summary>
		public static readonly Delegate FromNumber;
		/// <summary>
		/// Comparer for <typeparamref name="EnumT"/> values.
		/// </summary>
		public static readonly Comparer<EnumT> Comparer;
		/// <summary>
		/// Type code of enum underlying type.
		/// </summary>
		public static readonly TypeCode TypeCode;
		/// <summary>
		/// Type of enum underlying type.
		/// </summary>
		public static readonly Type UnderlyingType;
		/// <summary>
		/// Flag indicating what enum has <see cref="FlagsAttribute"/>.
		/// </summary>
		public static readonly bool IsFlags;
		/// <summary>
		/// Flag indicating what Enum <see cref="UnderlyingType"/> is signed number.
		/// </summary>
		public static readonly bool IsSigned;
		/// <summary>
		/// Maximum value for enumeration (declared values only).
		/// </summary>
		public static readonly EnumT MaxValue;
		/// <summary>
		/// Default value for enumeration (zero).
		/// </summary>
		public static readonly EnumT DefaultValue;
		/// <summary>
		/// Minimum value for enumeration (declared values only).
		/// </summary>
		public static readonly EnumT MinValue;
		/// <summary>
		/// Names of all enumeration values. Order is corresponding to <see cref="Values"/>.
		/// </summary>
		public static readonly ReadOnlyCollection<string> Names;
		/// <summary>
		/// All declared enumeration values. Order is corresponding to <see cref="Names"/>. 
		/// </summary>
		public static readonly ReadOnlyCollection<EnumT> Values;
		// ReSharper restore StaticMemberInGenericType

		static EnumHelper()
		{
			var enumType = typeof(EnumT);
#if NETSTANDARD
			var enumTypeInfo = enumType.GetTypeInfo();
#else
			var enumTypeInfo = enumType;
#endif
			if (enumTypeInfo.IsEnum == false)
				throw new InvalidOperationException("EnumT should be enum type.");

			var underlyingType = Enum.GetUnderlyingType(enumType);
			var valueParameter = Expression.Parameter(underlyingType, "value");
			var enumParameter = Expression.Parameter(enumType, "value");
			var xParameter = Expression.Parameter(enumType, "value");
			var yParameter = Expression.Parameter(enumType, "value");
			var instance = Activator.CreateInstance(enumType);

			UnderlyingType = underlyingType;
			TypeCode = Convert.GetTypeCode(instance);
			IsFlags = enumTypeInfo.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
			IsSigned = TypeCode == TypeCode.SByte || TypeCode == TypeCode.Int16 || TypeCode == TypeCode.Int32 || TypeCode == TypeCode.Int64;
			FromNumber = Expression.Lambda(Expression.ConvertChecked(valueParameter, enumType), valueParameter).Compile();
			ToNumber = Expression.Lambda(Expression.ConvertChecked(enumParameter, underlyingType), enumParameter).Compile();
			Comparer = new ComparisonComparer<EnumT>(Expression.Lambda<Comparison<EnumT>>(
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

			NamesByValue = new SortedDictionary<EnumT, string>(Comparer);
			ValueByName = new SortedDictionary<string, EnumT>(StringComparer.Ordinal);
			ValueByNameCaseInsensitive = new SortedDictionary<string, EnumT>(StringComparer.OrdinalIgnoreCase);
			DefaultValue = default(EnumT);

			var valuesArray = Enum.GetValues(enumType);
			var names = new List<string>(valuesArray.Length);
			var values = new List<EnumT>(valuesArray.Length);
			foreach (EnumT value in valuesArray)
			{
				var name = Enum.GetName(enumType, value);
				if (string.IsNullOrEmpty(name))
					continue;

				NamesByValue[value] = name;
				ValueByName[name] = value;
				ValueByNameCaseInsensitive[name] = value;
				names.Add(name);
				values.Add(value);

				MinValue = Comparer.Compare(value, MinValue) < 0 ? value : MinValue;
				MaxValue = Comparer.Compare(value, MaxValue) > 0 ? value : MaxValue;
			}

			if (values.Contains(DefaultValue) == false)
			{
				// if default value is not part of Values then
				// swap MinValue and MaxValue and re-calculate them
				var tempMaxValue = MaxValue;
				MaxValue = MinValue;
				MinValue = tempMaxValue;

				foreach (var value in values)
				{
					MinValue = Comparer.Compare(value, MinValue) < 0 ? value : MinValue;
					MaxValue = Comparer.Compare(value, MaxValue) > 0 ? value : MaxValue;
				}
			}

			Values = values.AsReadOnly();
			Names = names.AsReadOnly();
		}

		/// <summary>
		/// Get name of passed enumeration value.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns>Cached name of enumeration member or string representation of it's underlying type.</returns>
		public static string ToName(EnumT value)
		{
			if (NamesByValue.TryGetValue(value, out var name))
				return name;

			return Convert.ToString(value);
		}

		/// <summary>
		/// Convert passed enumeration value to <see cref="SByte"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="SByte"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="SByte"/> value of passed enumeration value.</returns>
		public static sbyte ToSByte(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (sbyte)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (sbyte)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (sbyte)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (sbyte)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (sbyte)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (sbyte)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (sbyte)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (sbyte)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Byte"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Byte"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Byte"/> value of passed enumeration value.</returns>
		public static byte ToByte(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (byte)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (byte)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (byte)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (byte)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (byte)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (byte)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (byte)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (byte)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int16"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int16"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Int16"/> value of passed enumeration value.</returns>
		public static short ToInt16(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (short)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (short)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (short)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (short)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (short)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (short)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (short)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (short)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt16"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt16"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="UInt16"/> value of passed enumeration value.</returns>
		public static ushort ToUInt16(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (ushort)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (ushort)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (ushort)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (ushort)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (ushort)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (ushort)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (ushort)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (ushort)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int32"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int32"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Int32"/> value of passed enumeration value.</returns>
		public static int ToInt32(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (int)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (int)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (int)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (int)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (int)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (int)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (int)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (int)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt32"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt32"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="UInt32"/> value of passed enumeration value.</returns>
		public static uint ToUInt32(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (uint)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (uint)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (uint)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (uint)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (uint)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (uint)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (uint)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (uint)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Int64"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Int64"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Int64"/> value of passed enumeration value.</returns>
		public static long ToInt64(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (long)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (long)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (long)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (long)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (long)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (long)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (long)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (long)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="UInt64"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="UInt64"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="UInt64"/> value of passed enumeration value.</returns>
		public static ulong ToUInt64(EnumT value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return (ulong)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
					case TypeCode.Byte: return (ulong)((Func<EnumT, byte>)ToNumber).Invoke(value);
					case TypeCode.Int16: return (ulong)((Func<EnumT, short>)ToNumber).Invoke(value);
					case TypeCode.UInt16: return (ulong)((Func<EnumT, ushort>)ToNumber).Invoke(value);
					case TypeCode.Int32: return (ulong)((Func<EnumT, int>)ToNumber).Invoke(value);
					case TypeCode.UInt32: return (ulong)((Func<EnumT, uint>)ToNumber).Invoke(value);
					case TypeCode.Int64: return (ulong)((Func<EnumT, long>)ToNumber).Invoke(value);
					case TypeCode.UInt64: return (ulong)((Func<EnumT, ulong>)ToNumber).Invoke(value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Single"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Single"/> value of passed enumeration value.</returns>
		public static float ToSingle(EnumT value)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (TypeCode)
			{
				case TypeCode.SByte: return (float)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
				case TypeCode.Byte: return (float)((Func<EnumT, byte>)ToNumber).Invoke(value);
				case TypeCode.Int16: return (float)((Func<EnumT, short>)ToNumber).Invoke(value);
				case TypeCode.UInt16: return (float)((Func<EnumT, ushort>)ToNumber).Invoke(value);
				case TypeCode.Int32: return (float)((Func<EnumT, int>)ToNumber).Invoke(value);
				case TypeCode.UInt32: return (float)((Func<EnumT, uint>)ToNumber).Invoke(value);
				case TypeCode.Int64: return (float)((Func<EnumT, long>)ToNumber).Invoke(value);
				case TypeCode.UInt64: return (float)((Func<EnumT, ulong>)ToNumber).Invoke(value);
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}
		}
		/// <summary>
		/// Convert passed enumeration value to <see cref="Double"/>. Throws <see cref="OverflowException"/> if value can't fit into <see cref="Double"/>.
		/// </summary>
		/// <param name="value">Enumeration value.</param>
		/// <returns><see cref="Double"/> value of passed enumeration value.</returns>
		public static double ToDouble(EnumT value)
		{
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (TypeCode)
			{
				case TypeCode.SByte: return (double)((Func<EnumT, sbyte>)ToNumber).Invoke(value);
				case TypeCode.Byte: return (double)((Func<EnumT, byte>)ToNumber).Invoke(value);
				case TypeCode.Int16: return (double)((Func<EnumT, short>)ToNumber).Invoke(value);
				case TypeCode.UInt16: return (double)((Func<EnumT, ushort>)ToNumber).Invoke(value);
				case TypeCode.Int32: return (double)((Func<EnumT, int>)ToNumber).Invoke(value);
				case TypeCode.UInt32: return (double)((Func<EnumT, uint>)ToNumber).Invoke(value);
				case TypeCode.Int64: return (double)((Func<EnumT, long>)ToNumber).Invoke(value);
				case TypeCode.UInt64: return (double)((Func<EnumT, ulong>)ToNumber).Invoke(value);
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}
		}

		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromSByte(sbyte value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromByte(byte value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromInt16(short value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration . Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromUInt16(ushort value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromInt32(int value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromUInt32(uint value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromInt64(long value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromUInt64(ulong value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromSingle(float value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}
		/// <summary>
		/// Convert passed number value to enumeration. Throws <see cref="OverflowException"/> if value can't fit into enumeration's underlying type.
		/// </summary>
		/// <param name="value">Numeric value.</param>
		/// <returns>Enumeration value from passed numeric value.</returns>
		public static EnumT FromDouble(double value)
		{
			checked
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (TypeCode)
				{
					case TypeCode.SByte: return ((Func<sbyte, EnumT>)FromNumber).Invoke((sbyte)value);
					case TypeCode.Byte: return ((Func<byte, EnumT>)FromNumber).Invoke((byte)value);
					case TypeCode.Int16: return ((Func<short, EnumT>)FromNumber).Invoke((short)value);
					case TypeCode.UInt16: return ((Func<ushort, EnumT>)FromNumber).Invoke((ushort)value);
					case TypeCode.Int32: return ((Func<int, EnumT>)FromNumber).Invoke((int)value);
					case TypeCode.UInt32: return ((Func<uint, EnumT>)FromNumber).Invoke((uint)value);
					case TypeCode.Int64: return ((Func<long, EnumT>)FromNumber).Invoke((long)value);
					case TypeCode.UInt64: return ((Func<ulong, EnumT>)FromNumber).Invoke((ulong)value);
					default:
						throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
				}
			}
		}

		/// <summary>
		/// Check if passed enumeration value is defined in enumeration.
		/// </summary>
		/// <param name="value">Enumeration value to check.</param>
		/// <returns>True if defined. False if not defined.</returns>
		public static bool IsDefined(EnumT value)
		{
			if (IsFlags)
			{
				return Enum.IsDefined(typeof(EnumT), value);
			}
			else
			{
				return NamesByValue.ContainsKey(value);
			}
		}
		/// <summary>
		/// Map passed <paramref name="name"/> to enumeration member name and return it's value. Or parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <returns>Enumeration value.</returns>
		public static EnumT Parse(string name)
		{
			return Parse(name, ignoreCase: false);
		}
		/// <summary>
		/// Map passed <paramref name="name"/> to enumeration member name and return it's value. Or parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <param name="ignoreCase">Ignore case of enumerable names during parsing.</param>
		/// <returns>Enumeration value.</returns>
		public static EnumT Parse(string name, bool ignoreCase)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var byNameMap = ignoreCase ? ValueByNameCaseInsensitive : ValueByName;
			if (byNameMap.TryGetValue(name, out var value))
				return value;

			if (TryParseNumber(name, out value))
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
		public static bool TryParse(string name, out EnumT value)
		{
			return TryParse(name, out value, ignoreCase: false);
		}
		/// <summary>
		/// Try to map passed <paramref name="name"/> to enumeration member name and return it's value. Or try to parse <paramref name="name"/> as number and maps it to first matching enumeration member.
		/// </summary>
		/// <param name="name">Enumeration member name-or-Enumeration member's numeric value-or-Enumeration member names separated by comma.</param>
		/// <param name="value">Mapped enumeration value.</param>
		/// <param name="ignoreCase">Ignore case of enumerable names during parsing.</param>
		/// <returns>True if mapped successfully. False if mapping failed.</returns>
		public static bool TryParse(string name, out EnumT value, bool ignoreCase)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			value = default(EnumT);

			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			var byNameMap = ignoreCase ? ValueByNameCaseInsensitive : ValueByName;
			if (byNameMap.TryGetValue(name, out value))
				return true;

			try
			{
				if (TryParseNumber(name, out value))
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

		private static bool TryParseNumber(string number, out EnumT value)
		{
			if (number == null) throw new ArgumentNullException(nameof(number));

			// ReSharper disable once SwitchStatementMissingSomeCases
			switch (TypeCode)
			{
				case TypeCode.SByte:
					if (sbyte.TryParse(number, out var int8Value))
					{
						value = ((Func<sbyte, EnumT>)FromNumber).Invoke(int8Value);
						return true;
					}
					break;
				case TypeCode.Byte:
					if (byte.TryParse(number, out var uint8Value))
					{
						value = ((Func<byte, EnumT>)FromNumber).Invoke(uint8Value);
						return true;
					}
					break;
				case TypeCode.Int16:
					if (short.TryParse(number, out var int16Value))
					{
						value = ((Func<short, EnumT>)FromNumber).Invoke(int16Value);
						return true;
					}
					break;
				case TypeCode.UInt16:
					if (ushort.TryParse(number, out var uint16Value))
					{
						value = ((Func<ushort, EnumT>)FromNumber).Invoke(uint16Value);
						return true;
					}
					break;
				case TypeCode.Int32:
					if (int.TryParse(number, out var int32Value))
					{
						value = ((Func<int, EnumT>)FromNumber).Invoke(int32Value);
						return true;
					}
					break;
				case TypeCode.UInt32:
					if (uint.TryParse(number, out var uint32Value))
					{
						value = ((Func<uint, EnumT>)FromNumber).Invoke(uint32Value);
						return true;
					}
					break;
				case TypeCode.Int64:
					if (long.TryParse(number, out var int64Value))
					{
						value = ((Func<long, EnumT>)FromNumber).Invoke(int64Value);
						return true;
					}
					break;
				case TypeCode.UInt64:
					if (ulong.TryParse(number, out var uint64Value))
					{
						value = ((Func<ulong, EnumT>)FromNumber).Invoke(uint64Value);
						return true;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException($"Invalid value '{TypeCode}' of type code of '{typeof(EnumT)}' enum.");
			}

			value = default(EnumT);
			return false;
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
