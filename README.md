Introduction
============
For historical reasons, .NET has several approaches to value conversion:
- Explicit/Implicit operators
- System.Convert class 
- IConvertible interface
- System.ComponentModel.TypeConverter
- To, From, Parse, Create methods
As well as a few special cases with meta types(Enums, Nullable Types).

TypeConvert combines all these approaches under one API. 
Additionally this library has methods for encoding data into hexadecimal representation and class instantiation.
Each class can be used separately and you are free to embedd them into your project.

#### TypeConvert
```csharp
// generic
ToType Convert<FromType, ToType>(value, [format], [formatProvider]);
bool TryConvert<FromType, ToType>(value, out result, [format], [formatProvider])
string ToString<FromType>(value, [format], [formatProvider]);
// non-generic
object Convert(fromType, toType, value, [format], [formatProvider]);
bool TryConvert(fromType, toType, ref value, [format], [formatProvider]);
string ToString(value, [format], [formatProvider]);
```

#### TypeActivator
```csharp
object CreateInstance(type, forceCreate = false);
object CreateInstance<Arg1T, ...>(type, arg1 ...);
```

#### HexConvert
```csharp
string BufferToHexString(buffer, offset, count); // bytes -> hex
byte[] HexStringToBuffer(hexString, offset, count); // hex -> bytes

int ToHex(value, hexBuffer, offset); // number -> hex
Number ToNumber(hexBuffer, offset) // hex -> number
```

Installation
============
```
Install-Package TypeConvert 
```

Examples
========
## TypeConvert
Convert from string to integer
```csharp
TypeConvert.Convert<string, int>("1") // 1
```	
Convert from integer to Enum
```csharp
TypeConvert.Convert<int, ConsoleColor>(1) // DarkBlue
```	
Convert from string to IPAddress
```csharp
TypeConvert.Convert<string, IPAddress>("127.0.0.1") // 127.0.0.1 via IPAddress.Parse
```
Convert to string with format(if supported)
```csharp
TypeConvert.ToString(1000000, format: "x") // f4240
```	
Convert from any to IpAddress
```csharp
TypeConvert.Convert(typeof(object), typeof(IPAddress), "127.0.0.1"); 127.0.0.1 via IPAddress.Parse
```
Testing conversion
```csharp
TypeConvert.TryConvert<string, int>("xxx", out intValue) // false
```
## TypeActivator
Creating from default constructor
```csharp
TypeActivator.CreateInstance(typeof(int)); // 0
```
Getting class instance without default constructor
```csharp
TypeActivator.CreateInstance(typeof(EventArgs)); // via EventArgs.Empty
```
Getting class instance without default constructor and Empty property
```csharp
TypeActivator.CreateInstance(typeof(IPEndPoint), force: true); // IPEndPoint bypassing constructor
```
Creating an array
```csharp
TypeActivator.CreateInstance(typeof(int[])); // int[0] (same instance every time)
```