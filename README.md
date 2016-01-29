Introduction
============
For historical reasons, .NET has several approaches to value conversion:
- Explicit/Implicit operators
- System.Convert class 
- IConvertible interface
- System.ComponentModel.TypeConverter
- To*, From*, Parse methods
As well as a few special cases with meta types(Enums, Nullable Types).

TypeConvert combines all these approaches under one API. 
Additionally this library has methods for encoding data into hexadecimal representation and class instantiation.
Each class can be used separately and you are free to embedd them into your project.

#### TypeConvert
```csharp
// generic
public static ToType Convert<FromType, ToType>(FromType value, string format = null, IFormatProvider formatProvider = null);
public static bool TryConvert<FromType, ToType>(FromType value, out ToType result, string format = null, IFormatProvider formatProvider = null)
public static string ToString<FromType>(FromType value, string format = null, IFormatProvider formatProvider = null);
// non-generic
public static object Convert(Type fromType, Type toType, object value, string format = null, IFormatProvider formatProvider = null);
public static bool TryConvert(Type fromType, Type toType, ref object value, string format = null, IFormatProvider formatProvider = null);
public static string ToString(object value, string format = null, IFormatProvider formatProvider = null);
```

#### TypeActivator
```csharp
public static object CreateInstance(Type type, bool forceCreate = false);
public static object CreateInstance<Arg1T, ...>(Type type, Arg1T arg1 ...);
```

#### HexConvert
```csharp
public static string BufferToHexString(byte[] buffer, int offset, int count); // bytes -> hex
public static byte[] HexStringToBuffer(string hexString, int offset, int count); // hex -> bytes

public static int ToHex(Number value, char[] hexBuffer, int offset); // number -> hex
public static Number ToNumber(char[] hexBuffer, int offset) // hex -> number
```

Installation
============
```
Install-Package System.TypeConvert 
```

Examples
========
## TypeConvert
#### Convert from string to integer
```csharp
TypeConvert.Convert<string, int>("1") // 1
```	
#### Convert from integer to Enum
```csharp
TypeConvert.Convert<int, ConsoleColor>(1) // DarkBlue
```	
#### Convert from string to IPAddress
```csharp
TypeConvert.Convert<string, IPAddress>("127.0.0.1") // 127.0.0.1 via IPAddress.Parse
```
#### Convert to string with format(if supported)
```csharp
TypeConvert.ToString(1000000, format: "x") // f4240
```	
#### Convert from any to IpAddress
```csharp
TypeConvert.Convert(typeof(object), typeof(IPAddress), "127.0.0.1"); 127.0.0.1 via IPAddress.Parse
```
#### Try convert
```csharp
TypeConvert.TryConvert<string, int>("xxx", out intValue) // false
```
## TypeActivator
#### Creating from default constructor
```csharp
TypeActivator.CreateInstance(typeof(int)); // 0
```
#### Getting class instance without default constructor
```csharp
TypeActivator.CreateInstance(typeof(EventArgs)); // via EventArgs.Empty
```
#### Getting class instance without default constructor and Empty property
```csharp
TypeActivator.CreateInstance(typeof(IPEndPoint), force: true); // IPEndPoint bypassing constructor
```
#### Creating an array
```csharp
TypeActivator.CreateInstance(typeof(int[])); // int[0] (same instance every time)
```