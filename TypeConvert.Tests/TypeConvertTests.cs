using System;
using Xunit;

namespace TypeConvert.Tests
{
	public sealed class TypeConvertTests
	{
		[Fact]
		public void ObjectToValueTypeTest()
		{
			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = System.TypeConvert.Convert<object, TimeSpan>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToNullableTest()
		{
			var timeSpanString = "00:00:01";
			var expected = TimeSpan.Parse(timeSpanString);
			var actual = System.TypeConvert.Convert<object, TimeSpan?>(timeSpanString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToNullableTest()
		{
			var value = default(object);
			var expected = default(TimeSpan?);
			var actual = System.TypeConvert.Convert<object, TimeSpan?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableToObjectTest()
		{
			var value = default(int?);
			var expected = default(object);
			var actual = System.TypeConvert.Convert<int?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToEnumTest()
		{
			var value = "Green";
			var expected = ConsoleColor.Green;
			var actual = System.TypeConvert.Convert<object, ConsoleColor>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToObjectTest()
		{
			var value = ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = System.TypeConvert.Convert<ConsoleColor, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToObjectTest()
		{
			var value = (ConsoleColor?)ConsoleColor.Green;
			var expected = ConsoleColor.Green;
			var actual = System.TypeConvert.Convert<ConsoleColor?, object>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToNullableEnumTest()
		{
			var value = "Green";
			var expected = (ConsoleColor?)ConsoleColor.Green;
			var actual = System.TypeConvert.Convert<object, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToNullableEnumTest()
		{
			var value = default(object);
			var expected = default(ConsoleColor?);
			var actual = System.TypeConvert.Convert<object, ConsoleColor?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ObjectToObjectTest()
		{
			var expected = "00:00:01";
			var actual = System.TypeConvert.Convert<object, object>(expected);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullObjectToClassTest()
		{
			var value = default(object);
			var expected = default(EventArgs);
			var actual = System.TypeConvert.Convert<object, EventArgs>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableToNullableTest()
		{
			var value = 1;
			var expected = (int?)1;
			var actual = System.TypeConvert.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public void NullableToNullableNullTest()
		{
			var value = default(long?);
			var expected = default(int?);
			var actual = System.TypeConvert.Convert<long?, int?>(value);

			Assert.Equal(expected, actual);

		}

		[Fact]
		public void EnumToNullableTest()
		{
			var value = ConsoleColor.DarkYellow;
			var expected = (int?)(int)value;
			var actual = System.TypeConvert.Convert<ConsoleColor, int?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToEnumTest()
		{
			var value = ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = System.TypeConvert.Convert<ConsoleColor, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToEnumTest()
		{
			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = CollectionBehavior.CollectionPerClass;
			var actual = System.TypeConvert.Convert<ConsoleColor?, CollectionBehavior>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void EnumToNullableEnumTest()
		{
			var value = ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			var actual = System.TypeConvert.Convert<ConsoleColor, CollectionBehavior?>(value);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void NullableEnumToNullableEnumTest()
		{
			var value = (ConsoleColor?)ConsoleColor.DarkBlue;
			var expected = (CollectionBehavior?)CollectionBehavior.CollectionPerClass;
			var actual = System.TypeConvert.Convert<ConsoleColor?, CollectionBehavior?>(value);

			Assert.Equal(expected, actual);
		}
	}
}
