﻿/*
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
			var conversionProvider = new TypeConversionProvider();
			var testMethodInfo = new Action<ITypeConversionProvider>(this.FromToMethodsTestImpl<ByteEnum, Byte>).Method.GetGenericMethodDefinition();
			var testMethod = (Action<ITypeConversionProvider>)Delegate.CreateDelegate(typeof(Action<ITypeConversionProvider>), this, testMethodInfo.MakeGenericMethod(enumType, underlyingType), throwOnBindFailure: true);
			testMethod?.Invoke(conversionProvider);
		}

		[Fact]
		public void TestParse()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var actual = enumConversionInfo.Parse(expected.ToString());

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestParseCaseInsensitie()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var actual = enumConversionInfo.Parse(expected.ToString().ToLowerInvariant(), ignoreCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParse()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var parsed = enumConversionInfo.TryParse(expected.ToString(), out var actual);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParseIgnoreCase()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var parsed = enumConversionInfo.TryParse(expected.ToString().ToLowerInvariant(), out var actual, ignoreCase: true);

			Assert.True(parsed);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestTryParseFailCase()
		{
			var enumConversionInfo = new EnumConversionInfo<ByteEnum>(useDynamicMethods: false);
			var expected = ByteEnum.One;
			var parsed = enumConversionInfo.TryParse(expected.ToString().ToLowerInvariant(), out var actual);

			Assert.False(parsed);
		}

		[Fact]
		public void TestTryParseFailValue()
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