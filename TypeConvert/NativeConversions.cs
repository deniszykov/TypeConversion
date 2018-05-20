

// ReSharper disable All
namespace System
{
	partial class TypeConvert
	{
		public const string CheckedConversionFormat = "checked";
		public const string UncheckedConversionFormat = "unchecked";

		private static void InitializeNativeConversions()
		{
			TypeConversion<System.Byte, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Byte, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Byte, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Byte, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Byte, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Byte, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Byte, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Byte, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.Byte, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.Byte, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.SByte, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.SByte, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.SByte, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.SByte, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.SByte, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.SByte, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.SByte, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.SByte, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.SByte, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.SByte, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.Int16, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Int16, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Int16, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Int16, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Int16, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Int16, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Int16, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Int16, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.Int16, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.Int16, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.UInt16, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.UInt16, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.UInt16, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.UInt16, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.UInt16, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.UInt16, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.UInt16, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.UInt16, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.UInt16, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.UInt16, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.Int32, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Int32, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Int32, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Int32, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Int32, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Int32, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Int32, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Int32, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.Int32, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.Int32, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.UInt32, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.UInt32, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.UInt32, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.UInt32, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.UInt32, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.UInt32, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.UInt32, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.UInt32, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.UInt32, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.UInt32, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.Int64, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Int64, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Int64, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Int64, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Int64, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Int64, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Int64, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Int64, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.Int64, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.Int64, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.UInt64, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.UInt64, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.UInt64, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.UInt64, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.UInt64, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.UInt64, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.UInt64, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.UInt64, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.UInt64, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.UInt64, System.Char>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v);
			TypeConversion<System.Double, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Double, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Double, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Double, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Double, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Double, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Double, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Double, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Double, System.Single>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v);
			TypeConversion<System.Single, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Single, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Single, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Single, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Single, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Single, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Single, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Single, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
			TypeConversion<System.Single, System.Double>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v);
			TypeConversion<System.Char, System.Byte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v);
			TypeConversion<System.Char, System.SByte>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v);
			TypeConversion<System.Char, System.Int16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v);
			TypeConversion<System.Char, System.UInt16>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v);
			TypeConversion<System.Char, System.Int32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v);
			TypeConversion<System.Char, System.UInt32>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v);
			TypeConversion<System.Char, System.Int64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v);
			TypeConversion<System.Char, System.UInt64>.NativeConversionFn = (v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v);
		}
	}
}