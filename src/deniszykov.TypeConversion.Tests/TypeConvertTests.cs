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
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace deniszykov.TypeConversion.Tests
{
	public sealed class TypeConvertTests
	{
		private static readonly ConversionOptions[] TestConversionOptions = new[]
		{
			ConversionOptions.FastCast,
			ConversionOptions.SkipComponentModelTypeConverters,
			ConversionOptions.OptimizeWithExpressions,
			ConversionOptions.OptimizeWithGenerics,
			ConversionOptions.OptimizeWithGenerics | ConversionOptions.OptimizeWithExpressions,
			ConversionOptions.None,
			ConversionOptions.UseDefaultFormatIfNotSpecified,
			ConversionOptions.UseDefaultFormatProviderIfNotSpecified,
			ConversionOptions.UseDefaultFormatProviderIfNotSpecified | ConversionOptions.UseDefaultFormatIfNotSpecified,
		};

		private readonly ITestOutputHelper outputHelper;

		public TypeConvertTests(ITestOutputHelper outputHelper)
		{
			this.outputHelper = outputHelper;
		}

		public static IEnumerable<object[]> ConversionOptionsTestData()
		{
			return
			(
				from option in TestConversionOptions
				select new object[] { option }
			);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToValueTypeTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = conversionProvider.Convert<object, TimeSpan>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToNullableTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = conversionProvider.Convert<object, TimeSpan?>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToNullableTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(TimeSpan?);
			var actual = conversionProvider.Convert<object?, TimeSpan?>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToObjectTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(int?);
			var expected = default(object);
			var actual = conversionProvider.Convert<int?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "Green";
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<object, ConsoleColor>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToObjectTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<ConsoleColor, object>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToObjectTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = (ConsoleColor?)ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = conversionProvider.Convert<ConsoleColor?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToNullableEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "Green";
			var expected = (ConsoleColor?)ConsoleColor.Green;
			var actual = conversionProvider.Convert<object, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToNullableEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(ConsoleColor?);
			var actual = conversionProvider.Convert<object?, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToObjectTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = "00:00:01";
			var actual = conversionProvider.Convert<object, object>(expected);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToClassTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(EventArgs);
			var actual = conversionProvider.Convert<object?, EventArgs>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToNullableTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = 1;
			var expected = (int?)1;
			var actual = conversionProvider.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToNullableNullTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(long?);
			var expected = default(int?);
			var actual = conversionProvider.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToNullableTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkYellow;
			var expected = (int?)(int)value;
			var actual = conversionProvider.Convert<ConsoleColor, int?>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToStringTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "DarkBlue";
			var expected = ConsoleColor.DarkBlue;
			var actual = conversionProvider.Convert<string, ConsoleColor>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = "DarkBlue";
			var actual = conversionProvider.Convert<ConsoleColor, string>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor?, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToNullableEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			var actual = conversionProvider.Convert<ConsoleColor, CollectionBehavior?>(value);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToNullableEnumTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

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
				from option in TestConversionOptions
				select new object[] { number, option }
			);
		}

		[Theory]
		[MemberData(nameof(NumbersTestData))]
		public void ConvertNumberToStringTest(object expected, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

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
				from option in TestConversionOptions
				select new object[] { fromType, toType, option }
			);
		}

		[Theory]
		[MemberData(nameof(NumberTypesTestData))]
		public void ConvertNumbersTest(Type fromType, Type toType, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var fromValue = conversionProvider.Convert(1.GetType(), fromType, 1);
			var expected = conversionProvider.Convert(1.GetType(), toType, 1);

			Assert.NotNull(fromValue);

			var actual = conversionProvider.Convert(fromValue!.GetType(), toType, fromValue);

			Assert.NotNull(expected);
			Assert.NotNull(actual);

			Assert.Equal(expected!.GetType(), actual!.GetType());
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToUrlTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Uri("http://exapmle.com/");
			var actual = conversionProvider.Convert<string, Uri>(expected.OriginalString);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToRelativeUrlTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Uri("/my", UriKind.Relative);
			var actual = conversionProvider.Convert<string, Uri>(expected.OriginalString);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToVersionTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Version(1, 0, 0, 0);
			var actual = conversionProvider.Convert<string, Version>("1.0.0.0");

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToDateTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

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

			return (
				from dt in dates
				from option in TestConversionOptions
				select new object[] { dt, option });
		}

		[Theory]
		[MemberData(nameof(DatesData))]
		public void DatesTest(DateTime expected, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var actual = conversionProvider.Convert<string, DateTime>(conversionProvider.Convert<DateTime, string>(expected));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullToStringTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var nullValue = default(object);
			var expected = default(string);
			var actual = conversionProvider.Convert<object, string>(nullValue);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToInvalidValueTypeTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var error = Assert.ThrowsAny<Exception>(() =>
			{
				var _ = conversionProvider.Convert<object, int>(new object());
			});

			this.outputHelper.WriteLine(error.ToString());
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToInvalidRefTypeTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var error = Assert.ThrowsAny<Exception>(() =>
			{
				var _ = conversionProvider.Convert<object, Version>(new object());
			});

			this.outputHelper.WriteLine(error.ToString());
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullToIntTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var error = Assert.ThrowsAny<Exception>(() =>
			{
				var _ = conversionProvider.Convert<object, int>(null);
			});

			this.outputHelper.WriteLine(error.ToString());
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void LongToIntTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var error = Assert.ThrowsAny<OverflowException>(() =>
			{
				var _ = conversionProvider.Convert<long, int>(long.MaxValue, TypeConversionProvider.CheckedConversionFormat);
			});

			this.outputHelper.WriteLine(error.ToString());
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void WrongStringToIntTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var error = Assert.ThrowsAny<FormatException>(() =>
			{
				var _ = conversionProvider.Convert<string, int>("aaa", TypeConversionProvider.CheckedConversionFormat);
			});

			this.outputHelper.WriteLine(error.ToString());
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void DateToStringTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options | ConversionOptions.PromoteValueToActualType
			}));

			var value = DateTime.Now;
			var expected = value.ToString("o");

			var actual = conversionProvider.Convert<object, string>(value, "o");

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToValueTypeTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			Assert.True(conversionProvider.TryConvert<object, TimeSpan>(timeSpanString, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToNullableTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			Assert.True(conversionProvider.TryConvert<object, TimeSpan?>(timeSpanString, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void InvalidObjectToNullableTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var intString = "4.9";
			Assert.False(conversionProvider.TryConvert<string, int?>(intString, out _));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToNullableTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(TimeSpan?);
			Assert.True(conversionProvider.TryConvert<object?, TimeSpan?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToObjectTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(int?);
			var expected = default(object);
			Assert.True(conversionProvider.TryConvert<int?, object>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "Green";
			var expected = ConsoleColor.Green;
			Assert.True(conversionProvider.TryConvert<object, ConsoleColor>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToObjectTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			Assert.True(conversionProvider.TryConvert<ConsoleColor, object>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToObjectTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = (ConsoleColor?)ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			Assert.True(conversionProvider.TryConvert<ConsoleColor?, object>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToNullableEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "Green";
			var expected = (ConsoleColor?)ConsoleColor.Green;
			Assert.True(conversionProvider.TryConvert<object, ConsoleColor?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToNullableEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(ConsoleColor?);
			Assert.True(conversionProvider.TryConvert<object?, ConsoleColor?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToObjectTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = "00:00:01";
			Assert.True(conversionProvider.TryConvert<object, object>(expected, out var actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullObjectToClassTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(object);
			var expected = default(EventArgs);
			Assert.True(conversionProvider.TryConvert<object?, EventArgs>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToNullableTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = 1;
			var expected = (int?)1;
			Assert.True(conversionProvider.TryConvert<long?, int?>(value, out var actual));

			Assert.Equal(expected, actual);

		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableToNullableNullTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = default(long?);
			var expected = default(int?);
			Assert.True(conversionProvider.TryConvert<long?, int?>(value, out var actual));

			Assert.Equal(expected, actual);

		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToNullableTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkYellow;
			var expected = (int?)(int)value;
			Assert.True(conversionProvider.TryConvert<ConsoleColor, int?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			Assert.True(conversionProvider.TryConvert<ConsoleColor, CollectionBehavior>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToStringTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = "DarkBlue";
			var expected = ConsoleColor.DarkBlue;
			Assert.True(conversionProvider.TryConvert<string, ConsoleColor>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = "DarkBlue";
			Assert.True(conversionProvider.TryConvert<ConsoleColor, string>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			Assert.True(conversionProvider.TryConvert<ConsoleColor?, CollectionBehavior>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void EnumToNullableEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			Assert.True(conversionProvider.TryConvert<ConsoleColor, CollectionBehavior?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullableEnumToNullableEnumTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			Assert.True(conversionProvider.TryConvert<ConsoleColor?, CollectionBehavior?>(value, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(NumbersTestData))]
		public void ConvertNumberToStringTryTest(object expected, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var stringValue = conversionProvider.Convert(expected.GetType(), typeof(string), expected);

			Assert.NotNull(stringValue);
			Assert.True(conversionProvider.TryConvert(stringValue!.GetType(), expected.GetType(), stringValue, out var actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.GetType(), actual!.GetType());
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(NumberTypesTestData))]
		public void ConvertNumbersTryTest(Type fromType, Type toType, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var fromValue = conversionProvider.Convert(1.GetType(), fromType, 1);
			var expected = conversionProvider.Convert(1.GetType(), toType, 1);

			Assert.NotNull(fromValue);

			Assert.True(conversionProvider.TryConvert(fromValue!.GetType(), toType, fromValue, out var actual));

			Assert.NotNull(expected);
			Assert.NotNull(actual);

			Assert.Equal(expected!.GetType(), actual!.GetType());
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToUrlTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Uri("http://exapmle.com/");
			Assert.True(conversionProvider.TryConvert<string, Uri>(expected.OriginalString, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToRelativeUrlTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Uri("/my", UriKind.Relative);
			Assert.True(conversionProvider.TryConvert<string, Uri>(expected.OriginalString, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToVersionTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = new Version(1, 0, 0, 0);
			Assert.True(conversionProvider.TryConvert<string, Version>("1.0.0.0", out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToDateTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var expected = DateTime.MinValue;
			Assert.True(conversionProvider.TryConvert<string, DateTime>(conversionProvider.Convert<DateTime, string>(expected), out var actual));

			Assert.Equal(expected, actual);
		}


		[Theory]
		[MemberData(nameof(DatesData))]
		public void DatesTryTest(DateTime expected, ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.True(conversionProvider.TryConvert<string, DateTime>(conversionProvider.Convert<DateTime, string>(expected), out var actual));
			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullToStringTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			var nullValue = default(object);
			var expected = default(string);
			Assert.True(conversionProvider.TryConvert<object, string>(nullValue!, out var actual));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToInvalidValueTypeTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.False(conversionProvider.TryConvert<object, int>(new object(), out _));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void ObjectToInvalidRefTypeTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.False(conversionProvider.TryConvert<object, Version>(new object(), out _));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void NullToIntTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.False(conversionProvider.TryConvert<object, int>(null!, out _));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void LongToIntTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.False(conversionProvider.TryConvert<long, int>(long.MaxValue, out _, TypeConversionProvider.CheckedConversionFormat));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void WrongStringToIntTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options
			}));

			Assert.False(conversionProvider.TryConvert<string, int>("aaa", out _, TypeConversionProvider.CheckedConversionFormat));
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void DateToStringTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options | ConversionOptions.PromoteValueToActualType
			}));

			var value = DateTime.Now;
			var expected = value.ToString("o");

			Assert.True(conversionProvider.TryConvert<object, string>(value, out var actual, "o"));

			Assert.Equal(expected, actual);
		}
		
		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void TimeSpanToStringTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options | ConversionOptions.PromoteValueToActualType
			}));

			var value = TimeSpan.FromSeconds(1);
			var expected = value.ToString("g");

			Assert.True(conversionProvider.TryConvert<object, string>(value, out var actual, "g"));

			Assert.Equal(expected, actual);
		}

		[Theory]
		[MemberData(nameof(ConversionOptionsTestData))]
		public void StringToTimeSpanTryTest(ConversionOptions options)
		{
			var conversionProvider = new TypeConversionProvider(Options.Create(new TypeConversionProviderOptions
			{
				Options = options | ConversionOptions.PromoteValueToActualType
			}));

			var value = "00:00:01";
			var expected = TimeSpan.Parse(value);

			Assert.True(conversionProvider.TryConvert<string, TimeSpan>(value, out var actual));

			Assert.Equal(expected, actual);
		}
	}
}
