Introduction
============
C# utilities for convertion between types, type construction and inspection

Installation
============
```
Install-Package TypeUtils 
```

Usage
============
Type convertion:
```csharp
using System;

// between types
TypeConvert.Convert<string, int>("1") // 1
TypeConvert.Convert<int, ConsoleColor>(1) // DarkBlue
TypeConvert.Convert<string, IPAddress>("127.0.0.1") // 127.0.0.1 via IPAddress.Parse
TypeConvert.Convert(typeof(object), typeof(int), "1") // 1
// with formatting
TypeConvert.Convert<int, string>(1000000, format: "x") // f4240
TypeConvert.ToString(1000000, format: "x") // f4240
```	
Construction:
```csharp
using System;

// calling default constructor or Empty property
TypeActivator.CreateInstance(typeof(int)); // 0
TypeActivator.CreateInstance(typeof(DateTime)); // new DateTime()
TypeActivator.CreateInstance(typeof(Guid)); // new Guid()
TypeActivator.CreateInstance(typeof(EventArgs)); // via EventArgs.Empty
TypeActivator.CreateInstance(typeof(int[])); // int[0] (same instance every time)
TypeActivator.CreateInstance(typeof(IPEndPoint), force: true); // IPEndPoint bypassing constructor
// calling constructor with argument (up to 3)
TypeActivator.CreateInstance(typeof(ArraySegment<byte>), new byte[20], 10, 10) // ArraySegment<byte> 
```	