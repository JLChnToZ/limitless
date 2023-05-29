# Limitless
This is a simple wrapper library that link up the power of Dynamic Language Runtime (DLR) and reflections. With this library, you can use simple code to access any fields, properties, methods, classes, instances, regardness of the type visibility. It is built on top of .NET Standard 2.0.

## Usage
In the below example code, even it demonstrated to deal with public methods of known types, it doesn't matter whether the subject is public or not.
```csharp
using JLChnToZ.CommonUtils.Dynamic;

// To construct an instance and call a method
var newString = Limitless.Construct(typeof(StringBuilder), "Hello, ").Append("World!").ToString();
// To call a static generic method, you may use .of(...) to specify generic type arguments.
Limitless.Static("UnityEngine.Object, UnityEngine").FindObjectOfType.of(typeof(Rigidbody))();
// To wrap an existing object.
var obj = new MyObject();
var objWrapped = Limitless.Wrap(obj);
// To re-wrap an object narrowed to a type, useful for accessing hidden interface members
var narrowedType = Lmitless.Wrap(obj, typeof(IMyInterface));
var narrowedType2 = Lmitless.Wrap(objWrapped, typeof(IMyInterface));
// To wrap a method to a delegate
Func<double, double> absWrapped = Limitless.Static(typeof(Math)).Abs;
```

## Installation
In the meanwhile there is no prebuilt binaries for distrube, but you may download the repository to build your own.

## License
[MIT](LICENSE)