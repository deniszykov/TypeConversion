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
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public sealed class TypeConvertTests
	{
		[Fact]
		public void ObjectToValueTypeTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = conversionProvider.Convert<object, TimeSpan>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToNullableTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = conversionProvider.Convert<object, TimeSpan?>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToNullableTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = default(object);
			var expected = default(TimeSpan?);
			var actual = conversionProvider.Convert<object?, TimeSpan?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableToObjectTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = default(int?);
			var expected = default(object);
			var actual = conversionProvider.Convert<int?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = "Green";
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<object, ConsoleColor>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToObjectTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<ConsoleColor, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToObjectTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = (ConsoleColor?)ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<ConsoleColor?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToNullableEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = "Green";
			var expected = (ConsoleColor?)ConsoleColor.Green;
			var actual = conversionProvider.Convert<object, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToNullableEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = default(object);
			var expected = default(ConsoleColor?);
			var actual = conversionProvider.Convert<object?, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToObjectTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var expected = "00:00:01";
			var actual = conversionProvider.Convert<object, object>(expected);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToClassTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = default(object);
			var expected = default(EventArgs);
			var actual = conversionProvider.Convert<object?, EventArgs>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableToNullableTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = 1;
			var expected = (int?)1;
			var actual = conversionProvider.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public void NullableToNullableNullTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = default(long?);
			var expected = default(int?);
			var actual = conversionProvider.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public void EnumToNullableTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = ConsoleColor.DarkYellow;
			var expected = (int?)(int)value;
			var actual = conversionProvider.Convert<ConsoleColor, int?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToStringTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = "DarkBlue";
			var expected = ConsoleColor.DarkBlue;
			var actual = conversionProvider.Convert<string, ConsoleColor>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void StringToEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = ConsoleColor.DarkBlue;
			var expected = "DarkBlue";
			var actual = conversionProvider.Convert<ConsoleColor, string>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor?, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToNullableEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor, CollectionBehavior?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToNullableEnumTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor?, CollectionBehavior?>(value);

			Assert.Equal(expected, actual);
		}

		public static IEnumerable<object[]> NumbersTestData()
		{
			var numbers = new object[]
			{
				// byte
				byte.MinValue,
				byte.MaxValue,
				(byte)1,
				(byte)8,
				(byte)10,
				(byte)15,
				(byte)16,
				(byte)17,
				(byte)31,
				(byte)32,
				(byte)33,
				(byte)63,
				(byte)64,
				(byte)65,
				(byte)66,
				(byte)127,
				(byte)128,
				
				// sbyte
				sbyte.MinValue,
				sbyte.MaxValue,
				(sbyte)0,
				(sbyte)1,
				(sbyte)8,
				(sbyte)10,
				(sbyte)15,
				(sbyte)16,
				(sbyte)17,
				(sbyte)31,
				(sbyte)32,
				(sbyte)33,
				(sbyte)63,
				(sbyte)64,
				(sbyte)65,
				(sbyte)66,

				// ushort
				ushort.MinValue,
				ushort.MaxValue,
				(ushort)1,
				(ushort)8,
				(ushort)15,
				(ushort)16,
				(ushort)31,
				(ushort)32,
				(ushort)33,
				(ushort)63,
				(ushort)64,
				(ushort)65,
				(ushort)127,
				(ushort)128,
				(ushort)129,
				(ushort)sbyte.MaxValue,
				(ushort)byte.MaxValue,
				(ushort)1024,
				(ushort)short.MaxValue,

				// short
				short.MinValue,
				short.MaxValue,
				(short)0,
				(short)1,
				(short)8,
				(short)15,
				(short)16,
				(short)31,
				(short)32,
				(short)33,
				(short)63,
				(short)64,
				(short)65,
				(short)127,
				(short)128,
				(short)129,
				(short)sbyte.MaxValue,
				(short)byte.MaxValue,
				(short)1024,

				// uint
				uint.MinValue,
				uint.MaxValue,
				(uint)1,
				(uint)8,
				(uint)15,
				(uint)16,
				(uint)31,
				(uint)32,
				(uint)33,
				(uint)63,
				(uint)64,
				(uint)65,
				(uint)127,
				(uint)128,
				(uint)129,
				(uint)sbyte.MaxValue,
				(uint)byte.MaxValue,
				(uint)1024,
				(uint)short.MaxValue,
				(uint)ushort.MaxValue,
				(uint)ushort.MaxValue * 2,
				(uint)int.MaxValue,

				// int
				int.MinValue,
				int.MaxValue,
				-257,
				-256,
				-255,
				-254,
				-128,
				-127,
				-126,
				-33,
				-32,
				-31,
				-16,
				-8,
				-2,
				-1,
				0,
				1,
				8,
				15,
				16,
				31,
				32,
				33,
				63,
				64,
				65,
				127,
				128,
				129,
				(int)sbyte.MaxValue,
				(int)byte.MaxValue,
				1024,
				(int)short.MaxValue,
				(int)ushort.MaxValue,

				// ulong
				ulong.MinValue,
				ulong.MaxValue,
				(ulong)1,
				(ulong)8,
				(ulong)15,
				(ulong)16,
				(ulong)31,
				(ulong)32,
				(ulong)33,
				(ulong)63,
				(ulong)64,
				(ulong)65,
				(ulong)127,
				(ulong)128,
				(ulong)129,
				(ulong)sbyte.MaxValue,
				(ulong)byte.MaxValue,
				(ulong)1024,
				(ulong)short.MaxValue,
				(ulong)ushort.MaxValue,
				(ulong)ushort.MaxValue * 2,
				(ulong)int.MaxValue,
				(ulong)uint.MaxValue,
				(ulong)uint.MaxValue * 2,
				(ulong)long.MaxValue,

				// long
				long.MinValue,
				long.MaxValue,
				(long)-256,
				(long)-128,
				(long)-127,
				(long)-32,
				(long)-8,
				(long)-1,
				(long)1,
				(long)8,
				(long)15,
				(long)16,
				(long)31,
				(long)32,
				(long)33,
				(long)63,
				(long)64,
				(long)65,
				(long)127,
				(long)128,
				(long)129,
				(long)sbyte.MaxValue,
				(long)byte.MaxValue,
				(long)1024,
				(long)short.MaxValue,
				(long)ushort.MaxValue,
				(long)ushort.MaxValue * 2,
				(long)int.MaxValue,
				(long)uint.MaxValue,
				(long)uint.MaxValue * 2,

				// float
				float.MinValue,
				float.MaxValue,
				(float)-256,
				(float)-128,
				(float)-127,
				(float)-32,
				(float)-8,
				(float)-1,
				(float)1,
				(float)8,
				(float)15,
				(float)16,
				(float)31,
				(float)32,
				(float)33,
				(float)63,
				(float)64,
				(float)65,
				(float)127,
				(float)128,
				(float)129,
				(float)sbyte.MaxValue,
				(float)byte.MaxValue,
				(float)1024,
				(float)short.MaxValue,
				(float)ushort.MaxValue,
				(float)ushort.MaxValue * 2,

				// double
				double.MinValue,
				double.MaxValue,
				(double)1,
				(double)8,
				(double)15,
				(double)16,
				(double)31,
				(double)32,
				(double)33,
				(double)63,
				(double)64,
				(double)65,
				(double)127,
				(double)128,
				(double)129,
				(double)sbyte.MaxValue,
				(double)byte.MaxValue,
				(double)1024,
				(double)short.MaxValue,
				(double)ushort.MaxValue,
				(double)ushort.MaxValue * 2,
				(double)int.MaxValue,
				(double)uint.MaxValue,
				(double)uint.MaxValue * 2,

				// decimal
				decimal.MinValue,
				decimal.MaxValue,
				(decimal)-256,
				(decimal)-128,
				(decimal)-127,
				(decimal)-32,
				(decimal)-8,
				(decimal)-1,
				(decimal)1,
				(decimal)8,
				(decimal)15,
				(decimal)16,
				(decimal)31,
				(decimal)32,
				(decimal)33,
				(decimal)63,
				(decimal)64,
				(decimal)65,
				(decimal)127,
				(decimal)128,
				(decimal)129,
				(decimal)sbyte.MaxValue,
				(decimal)byte.MaxValue,
				(decimal)1024,
				(decimal)short.MaxValue,
				(decimal)ushort.MaxValue,
				(decimal)ushort.MaxValue * 2,
				(decimal)int.MaxValue,
				(decimal)uint.MaxValue,
				(decimal)uint.MaxValue * 2,
			};

			return
			(
				from number in numbers
				select new object[] { number }
			);
		}

		[Theory]
		[MemberData(nameof(NumbersTestData))]
		public void ConvertNumberToStringTest(object expected)
		{
			var conversionProvider = new TypeConversionProvider();
			var stringValue = conversionProvider.Convert(expected.GetType(), typeof(string), expected);

			Assert.NotNull(stringValue);
			var actual = conversionProvider.Convert(stringValue!.GetType(), expected.GetType(), stringValue);

			Assert.NotNull(actual);
			Assert.Equal(expected.GetType(), actual!.GetType());
			Assert.Equal(expected, actual);
		}

		public static IEnumerable<object[]> NumberTypesTestData()
		{
			var numberTypes = new object[]
			{
				typeof(byte),
				typeof(sbyte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
			};

			return
			(
				from fromType in numberTypes
				from toType in numberTypes
				select new object[] { fromType, toType }
			);
		}

		[Theory]
		[MemberData(nameof(NumberTypesTestData))]
		public void ConvertNumbersTest(Type fromType, Type toType)
		{
			var conversionProvider = new TypeConversionProvider();
			var fromValue = conversionProvider.Convert(1.GetType(), fromType, 1);
			var expected = conversionProvider.Convert(1.GetType(), toType, 1);

			Assert.NotNull(fromValue);

			var actual = conversionProvider.Convert(fromValue!.GetType(), toType, fromValue);

			Assert.NotNull(expected);
			Assert.NotNull(actual);

			Assert.Equal(expected!.GetType(), actual!.GetType());
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void StringToUrlTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var expected = new Uri("http://exapmle.com/");
			var actual = conversionProvider.Convert<string, Uri>(expected.OriginalString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void StringToRelativeUrlTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var expected = new Uri("/my", UriKind.Relative);
			var actual = conversionProvider.Convert<string, Uri>(expected.OriginalString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void StringToVersionTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var expected = new Version(1, 0, 0, 0);
			var actual = conversionProvider.Convert<string, Version>("1.0.0.0");

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void StringToDateTest()
		{
			var conversionProvider = new TypeConversionProvider();
			var expected = DateTime.MinValue;
			var actual = conversionProvider.Convert<string, DateTime>(conversionProvider.Convert<DateTime, string>(expected));

			Assert.Equal(expected, actual);
		}

		public static IEnumerable<object[]> DatesData()
		{
			var dates = new[] {
				DateTime.MinValue,
				DateTime.MaxValue,
				DateTime.Now,
				DateTime.UtcNow,
				DateTime.Today,
				new DateTime(DateTime.Today.Ticks, DateTimeKind.Unspecified)
			};

			return (from dt in dates select new object[] { dt });
		}

		[Theory]
		[MemberData(nameof(DatesData))]
		public void DatesTest(DateTime expected)
		{
			var conversionProvider = new TypeConversionProvider();
			var actual = conversionProvider.Convert<string, DateTime>(conversionProvider.Convert<DateTime, string>(expected));

			Assert.Equal(expected, actual);
		}
	}
}
