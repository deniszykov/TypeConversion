/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TypeConvert.Tests
{
	public class TypeActivatorTest
	{
		[Theory]
		[InlineData(default(byte))]
		[InlineData(default(short))]
		[InlineData(default(int))]
		[InlineData(default(long))]
		[InlineData(default(sbyte))]
		[InlineData(default(ushort))]
		[InlineData(default(uint))]
		[InlineData(default(ulong))]
		[InlineData(default(float))]
		[InlineData(default(double))]
		public void CreateBuildInTypes(object expectedValue)
		{
			var actualValue = TypeActivator.CreateInstance(expectedValue.GetType());
			Assert.Equal(expectedValue, actualValue);
		}

		[Theory]
		[InlineData(typeof(DateTime))] // struct
		[InlineData(typeof(decimal))] // struct
		[InlineData(typeof(Guid))] // struct
		[InlineData(typeof(ConsoleColor))] // enum
		[InlineData(typeof(EventArgs))] // class with Empty property
		[InlineData(typeof(string))] // class with Empty property
		[InlineData(typeof(Exception))] // class
		[InlineData(typeof(int[]))] // array
		[InlineData(typeof(List<int>))] // class
		public void CreateType(Type expectedInstanceType)
		{
			var actualValue = TypeActivator.CreateInstance(expectedInstanceType);
			Assert.NotNull(actualValue);
			Assert.IsAssignableFrom(expectedInstanceType, actualValue);
		}

		[Fact]
		public void CreateInstanceWithArgs1()
		{
			var arr = new byte[0];
			var expected = new ArraySegment<byte>(arr);
			var actual = (ArraySegment<byte>)TypeActivator.CreateInstance(typeof(ArraySegment<byte>), arr);

			Assert.Equal(expected.Array, actual.Array);
			Assert.Equal(expected.Offset, actual.Offset);
			Assert.Equal(expected.Count, actual.Count);
		}

		[Fact]
		public void CreateInstanceWithArgs2()
		{
			var expected = new string('c', 10);
			var actual = (string)TypeActivator.CreateInstance(typeof(string), 'c', 10);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CreateInstanceWithArgs3()
		{
			var arr = new byte[20];
			var expected = new ArraySegment<byte>(arr, 10, 10);
			var actual = (ArraySegment<byte>)TypeActivator.CreateInstance(typeof(ArraySegment<byte>), arr, 10, 10);

			Assert.Equal(expected.Array, actual.Array);
			Assert.Equal(expected.Offset, actual.Offset);
			Assert.Equal(expected.Count, actual.Count);
		}

		[Fact]
		public void CreateInstanceWithArgs4()
		{
			var expected = Guid.Empty;
			var actual = (Guid)TypeActivator.CreateInstance(typeof(Guid), 0, (short)0, (short)0, new byte[8]);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CreateInstanceWithTypeCast()
		{
			var expectedStream = new MemoryStream();
			var writer = (StreamWriter)TypeActivator.CreateInstance(typeof(StreamWriter), expectedStream);

			Assert.Equal(expectedStream, writer.BaseStream);
		}
	}
}
