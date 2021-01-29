/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class EnumConversionInfoTest
	{
		private enum ByteEnum : byte { One = 1, Two = 2 }
		private enum SByteEnum : sbyte { One = 1, Two = 2 }
		private enum Int16Enum : short { One = 1, Two = 2 }
		private enum UInt16Enum : ushort { One = 1, Two = 2 }
		private enum Int32Enum : int { One = 1, Two = 2 }
		private enum UInt32Enum : uint { One = 1, Two = 2 }
		private enum Int64Enum : long { One = 1, Two = 2 }
		private enum UInt64Enum : ulong { One = 1, Two = 2 }
		[Flags]
		private enum FlagsEnum : ulong { One = 1, Two = 2 }

		public static IEnumerable<object[]> EnumHelperTestData()
		{
			var enumTypes = new[] { typeof(ByteEnum), typeof(SByteEnum), typeof(Int16Enum), typeof(UInt16Enum), typeof(Int32Enum), typeof(UInt32Enum), typeof(Int64Enum), typeof(UInt64Enum) };
			return (
				from enumType in enumTypes
				select new object[] { enumType, Enum.GetUnderlyingType(enumType) }
			);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ConstructorTest(bool useDynamicMethod)
		{
			var conversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethod);

			Assert.NotNull(conversionInfo.Comparer);
			Assert.NotNull(conversionInfo.FromNumber);
			Assert.NotNull(conversionInfo.ToNumber);
			Assert.NotNull(conversionInfo.Names);
			Assert.NotNull(conversionInfo.Values);
			Assert.NotNull(conversionInfo.Type);
			Assert.NotNull(conversionInfo.UnderlyingType);
			Assert.NotNull(conversionInfo.ToString());
			Assert.False(conversionInfo.IsFlags);
			Assert.False(conversionInfo.IsSigned);
			Assert.Equal(typeof(ByteEnum), conversionInfo.Type);
			Assert.Equal(typeof(byte), conversionInfo.UnderlyingType);
			Assert.Equal(TypeCode.Byte, conversionInfo.UnderlyingTypeCode);
			Assert.Equal(default(ByteEnum), conversionInfo.DefaultValue);
			Assert.Equal(ByteEnum.One, conversionInfo.MinValue);
			Assert.Equal(ByteEnum.Two, conversionInfo.MaxValue);
			Assert.Contains(conversionInfo.Names, v => v == nameof(ByteEnum.One));
			Assert.Contains(conversionInfo.Names, v => v == nameof(ByteEnum.Two));
			Assert.Contains(conversionInfo.Values, v => v == ByteEnum.One);
			Assert.Contains(conversionInfo.Values, v => v == ByteEnum.Two);
			Assert.IsType<Func<byte, ByteEnum>>(conversionInfo.FromNumber);
			Assert.IsType<Func<ByteEnum, byte>>(conversionInfo.ToNumber);
			Assert.Equal((byte)ByteEnum.One, ((Func<ByteEnum, byte>)conversionInfo.ToNumber).Invoke(ByteEnum.One));
			Assert.Equal(ByteEnum.One, ((Func<byte, ByteEnum>)conversionInfo.FromNumber).Invoke((byte)ByteEnum.One));
			Assert.Equal(-1, conversionInfo.Comparer.Compare(ByteEnum.One, ByteEnum.Two));
			Assert.Equal(0, conversionInfo.Comparer.Compare(ByteEnum.One, ByteEnum.One));
			Assert.Equal(1, conversionInfo.Comparer.Compare(ByteEnum.Two, ByteEnum.One));
		}

		[Theory]
		[MemberData(nameof(EnumHelperTestData))]
		public void ConstantsTest(Type enumType, Type underlyingType)
		{
			var conversionProvider = new TypeConversionProvider();
			var testMethodInfo = new Action<ITypeConversionProvider>(this.FromToMethodsTestImpl<ByteEnum, Byte>).Method.GetGenericMethodDefinition();
			var testMethod = (Action<ITypeConversionProvider>)Delegate.CreateDelegate(typeof(Action<ITypeConversionProvider>), this, testMethodInfo.MakeGenericMethod(enumType, underlyingType), throwOnBindFailure: true);
			testMethod?.Invoke(conversionProvider);
		}

		[Theory]
		[InlineData(ByteEnum.One)]
		[InlineData(SByteEnum.One)]
		[InlineData(Int16Enum.One)]
		[InlineData(UInt16Enum.One)]
		[InlineData(Int32Enum.One)]
		[InlineData(UInt32Enum.One)]
		[InlineData(Int64Enum.One)]
		[InlineData(UInt64Enum.One)]
		public void ParseTest<EnumT>(EnumT expected)
		{
			var enumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods: false);
			var actual = enumConversionInfo.Parse(expected.ToString());

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(ByteEnum.One)]
		[InlineData(SByteEnum.One)]
		[InlineData(Int16Enum.One)]
		[InlineData(UInt16Enum.One)]
		[InlineData(Int32Enum.One)]
		[InlineData(UInt32Enum.One)]
		[InlineData(Int64Enum.One)]
		[InlineData(UInt64Enum.One)]
		public void ParseNumberTest<EnumT>(EnumT expected)
		{
			var enumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods: false);
			var actual = enumConversionInfo.Parse("1");

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ParseFlagsTest()
		{
			var enumConversionInfo = new EnumConversionInfo<FlagsEnum>(useDynamicMethods: false);
			var expected = FlagsEnum.One | FlagsEnum.Two;
			var actual = enumConversionInfo.Parse("One, Two");

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsDefinedTest()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var defined = enumConversionInfo.IsDefined(ByteEnum.One);
			var notDefined = enumConversionInfo.IsDefined((ByteEnum)243);

			Assert.True(defined);
			Assert.False(notDefined);
		}

		[Theory]
		[InlineData(ByteEnum.One, true)]
		[InlineData(ByteEnum.One, false)]
		[InlineData(SByteEnum.One, true)]
		[InlineData(Int16Enum.One, true)]
		[InlineData(UInt16Enum.One, true)]
		[InlineData(Int32Enum.One, true)]
		[InlineData(UInt32Enum.One, true)]
		[InlineData(Int64Enum.One, true)]
		[InlineData(UInt64Enum.One, true)]
		public void ToMethodsTest<EnumT>(EnumT expected, bool useDynamicMethods)
		{
			var byteEnumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods);
			
			Assert.Equal((float)1, byteEnumConversionInfo.ToSingle(expected));
			Assert.Equal((double)1, byteEnumConversionInfo.ToDouble(expected));
			Assert.Equal((sbyte)1, byteEnumConversionInfo.ToSByte(expected));
			Assert.Equal((byte)1, byteEnumConversionInfo.ToByte(expected));
			Assert.Equal((short)1, byteEnumConversionInfo.ToInt16(expected));
			Assert.Equal((ushort)1, byteEnumConversionInfo.ToUInt16(expected));
			Assert.Equal((int)1, byteEnumConversionInfo.ToInt32(expected));
			Assert.Equal((uint)1, byteEnumConversionInfo.ToUInt32(expected));
			Assert.Equal((long)1, byteEnumConversionInfo.ToInt64(expected));
			Assert.Equal((ulong)1, byteEnumConversionInfo.ToUInt64(expected));
		}

		[Theory]
		[InlineData(ByteEnum.One, true)]
		[InlineData(ByteEnum.One, false)]
		[InlineData(SByteEnum.One, true)]
		[InlineData(Int16Enum.One, true)]
		[InlineData(UInt16Enum.One, true)]
		[InlineData(Int32Enum.One, true)]
		[InlineData(UInt32Enum.One, true)]
		[InlineData(Int64Enum.One, true)]
		[InlineData(UInt64Enum.One, true)]
		public void FromMethodsTest<EnumT>(EnumT expected, bool useDynamicMethods)
		{
			var byteEnumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods);
			
			Assert.Equal(expected, byteEnumConversionInfo.FromSingle(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromDouble(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromByte(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromSByte(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromInt16(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromUInt16(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromInt32(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromUInt32(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromInt64(1));
			Assert.Equal(expected, byteEnumConversionInfo.FromUInt64(1));
		}

		[Fact]
		public void ParseIgnoreCaseTest()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var actual = enumConversionInfo.Parse(expected.ToString().ToLower(), ignoreCase: true);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(ByteEnum.One)]
		[InlineData(SByteEnum.One)]
		[InlineData(Int16Enum.One)]
		[InlineData(UInt16Enum.One)]
		[InlineData(Int32Enum.One)]
		[InlineData(UInt32Enum.One)]
		[InlineData(Int64Enum.One)]
		[InlineData(UInt64Enum.One)]
		public void TryParseNumberTest<EnumT>(EnumT expected)
		{
			var byteEnumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods: false);
			var parsed = byteEnumConversionInfo.TryParse("1", out var actual);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(ByteEnum.One)]
		[InlineData(SByteEnum.One)]
		[InlineData(Int16Enum.One)]
		[InlineData(UInt16Enum.One)]
		[InlineData(Int32Enum.One)]
		[InlineData(UInt32Enum.One)]
		[InlineData(Int64Enum.One)]
		[InlineData(UInt64Enum.One)]
		public void TryParseTest<EnumT>(EnumT expected)
		{
			var enumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods: false);
			var parsed = enumConversionInfo.TryParse(expected.ToString(), out var actual);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TryParseFlagsTest()
		{
			var enumConversionInfo = new EnumConversionInfo<FlagsEnum>(useDynamicMethods: false);
			var expected = FlagsEnum.One | FlagsEnum.Two;
			var parsed = enumConversionInfo.TryParse(expected.ToString().ToLowerInvariant(), out var actual, ignoreCase: true);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TryParseIgnoreCaseTest()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var parsed = enumConversionInfo.TryParse(expected.ToString().ToLowerInvariant(), out var actual, ignoreCase: true);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TryParseFailCaseTest()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var parsed = enumConversionInfo.TryParse(expected.ToString().ToLowerInvariant(), out var actual);

			Assert.False(parsed);
		}

		[Fact]
		public void TryParseFailValueTest()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var parsed = enumConversionInfo.TryParse("WRONG", out var _);

			Assert.False(parsed);
		}

		private void FromToMethodsTestImpl<EnumT, UnderlyingT>(ITypeConversionProvider provider)
		{
			var enumConversionInfo = new EnumConversionInfo<EnumT>(useDynamicMethods: false);

			Assert.IsType<Func<EnumT, UnderlyingT>>(enumConversionInfo.ToNumber);
			Assert.IsType<Func<UnderlyingT, EnumT>>(enumConversionInfo.FromNumber);
			Assert.NotNull(enumConversionInfo.Comparer);
			Assert.Equal(Type.GetTypeCode(typeof(UnderlyingT)), enumConversionInfo.UnderlyingTypeCode);
			Assert.Equal(typeof(UnderlyingT), enumConversionInfo.UnderlyingType);
			Assert.Equal(typeof(EnumT).GetCustomAttribute<FlagsAttribute>() != null, enumConversionInfo.IsFlags);
			Assert.Equal(typeof(UnderlyingT) == typeof(sbyte) ||
				typeof(UnderlyingT) == typeof(short) ||
				typeof(UnderlyingT) == typeof(int) ||
				typeof(UnderlyingT) == typeof(long), enumConversionInfo.IsSigned);
			Assert.Equal(default(EnumT), enumConversionInfo.DefaultValue);
			Assert.Equal(provider.Convert<int, EnumT>(1), enumConversionInfo.MinValue);
			Assert.Equal(provider.Convert<int, EnumT>(2), enumConversionInfo.MaxValue);
			Assert.Equal(new[] { nameof(SByteEnum.One), nameof(SByteEnum.Two) }, enumConversionInfo.Names);
			Assert.Equal(new[] { provider.Convert<int, EnumT>(1), provider.Convert<int, EnumT>(2) }, enumConversionInfo.Values);
		}
	}
}
