

// ReSharper disable All
namespace System
{
	partial class TypeConvert
	{
		/// <summary>
		/// Format string enabling "checked" numeric conversion.
		/// </summary>
		public const string CheckedConversionFormat = "checked";
		/// <summary>
		/// Format string enabling "unchecked" numeric conversion.
		/// </summary>
		public const string UncheckedConversionFormat = "unchecked";

		private static void InitializeNativeConversions()
		{
			TypeConvert.RegisterCustomConversion<System.Byte, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.Byte, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.SByte, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.Int16, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.UInt16, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.Int32, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.UInt32, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.Int64, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.UInt64, System.Char>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Char)v) : unchecked((System.Char)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Double, System.Single>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Single)v) : unchecked((System.Single)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
			TypeConvert.RegisterCustomConversion<System.Single, System.Double>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Double)v) : unchecked((System.Double)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.Byte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Byte)v) : unchecked((System.Byte)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.SByte>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.SByte)v) : unchecked((System.SByte)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.Int16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int16)v) : unchecked((System.Int16)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.UInt16>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt16)v) : unchecked((System.UInt16)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.Int32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int32)v) : unchecked((System.Int32)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.UInt32>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt32)v) : unchecked((System.UInt32)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.Int64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.Int64)v) : unchecked((System.Int64)v));
			TypeConvert.RegisterCustomConversion<System.Char, System.UInt64>((v, f, fp) => (f == CheckedConversionFormat) ? checked((System.UInt64)v) : unchecked((System.UInt64)v));
		}
	}
}