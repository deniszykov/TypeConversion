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

namespace TypeConvert.Tests
{
	public class EnumHelperTest
	{
		private enum ByteEnum : byte { One = 1, Two = 2 }
		private enum SByteEnum : sbyte { One = 1, Two = 2 }
		private enum Int16Enum : short { One = 1, Two = 2 }
		private enum UInt16Enum : ushort { One = 1, Two = 2 }
		private enum Int32Enum : int { One = 1, Two = 2 }
		private enum UInt32Enum : uint { One = 1, Two = 2 }
		private enum Int64Enum : long { One = 1, Two = 2 }
		private enum UInt64Enum : uint { One = 1, Two = 2 }

		public static IEnumerable<object[]> EnumHelperTestData()
		{
			var enumTypes = new[] { typeof(ByteEnum), typeof(SByteEnum), typeof(Int16Enum), typeof(UInt16Enum), typeof(Int32Enum), typeof(UInt32Enum), typeof(Int64Enum), typeof(UInt64Enum) };
			return (
				from enumType in enumTypes
				select new object[] { enumType, Enum.GetUnderlyingType(enumType) }
			);
		}

		[Theory]
		[MemberData(nameof(EnumHelperTestData))]
		public void TestConstants(Type enumType, Type underlyingType)
		{
			var testMethodInfo = new Action(FromToMethodsTestImpl<ByteEnum, Byte>).Method.GetGenericMethodDefinition();
			var testMethod = (Action)Delegate.CreateDelegate(typeof(Action), this, testMethodInfo.MakeGenericMethod(enumType, underlyingType), throwOnBindFailure: true);
			testMethod?.Invoke();
		}

		[Fact]
		public void TestParse()
		{
			var expected = ByteEnum.One;
			var actual = EnumHelper<ByteEnum>.Parse(expected.ToString());

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestParseCaseInsensitie()
		{
			var expected = ByteEnum.One;
			var actual = EnumHelper<ByteEnum>.Parse(expected.ToString().ToLowerInvariant(), ignoreCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParse()
		{
			var expected = ByteEnum.One;
			var parsed = EnumHelper<ByteEnum>.TryParse(expected.ToString(), out var actual);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParseIgnoreCase()
		{
			var expected = ByteEnum.One;
			var parsed = EnumHelper<ByteEnum>.TryParse(expected.ToString().ToLowerInvariant(), out var actual, ignoreCase: true);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParseFailCase()
		{
			var expected = ByteEnum.One;
			var parsed = EnumHelper<ByteEnum>.TryParse(expected.ToString().ToLowerInvariant(), out var actual);

			Assert.False(parsed);
		}

		[Fact]
		public void TestTryParseFailValue()
		{
			var parsed = EnumHelper<ByteEnum>.TryParse("WRONG", out var _);

			Assert.False(parsed);
		}

		private void FromToMethodsTestImpl<EnumT, UnderlyingT>()
		{
			Assert.IsType<Func<EnumT, UnderlyingT>>(EnumHelper<EnumT>.ToNumber);
			Assert.IsType<Func<UnderlyingT, EnumT>>(EnumHelper<EnumT>.FromNumber);
			Assert.NotNull(EnumHelper<EnumT>.Comparer);
			Assert.Equal(Type.GetTypeCode(typeof(UnderlyingT)), EnumHelper<EnumT>.TypeCode);
			Assert.Equal(typeof(UnderlyingT), EnumHelper<EnumT>.UnderlyingType);
			Assert.Equal(typeof(EnumT).GetCustomAttribute<FlagsAttribute>() != null, EnumHelper<EnumT>.IsFlags);
			Assert.Equal(typeof(UnderlyingT) == typeof(sbyte) ||
				typeof(UnderlyingT) == typeof(short) ||
				typeof(UnderlyingT) == typeof(int) ||
				typeof(UnderlyingT) == typeof(long), EnumHelper<EnumT>.IsSigned);
			Assert.Equal(default(EnumT), EnumHelper<EnumT>.DefaultValue);
			Assert.Equal(System.TypeConvert.Convert<int, EnumT>(1), EnumHelper<EnumT>.MinValue);
			Assert.Equal(System.TypeConvert.Convert<int, EnumT>(2), EnumHelper<EnumT>.MaxValue);
			Assert.Equal(new[] { nameof(SByteEnum.One), nameof(SByteEnum.Two) }, EnumHelper<EnumT>.Names);
			Assert.Equal(new[] { System.TypeConvert.Convert<int, EnumT>(1), System.TypeConvert.Convert<int, EnumT>(2) }, EnumHelper<EnumT>.Values);
		}
	}
}
