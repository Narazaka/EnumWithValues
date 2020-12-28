# EnumWithValues

C# enum(like) with non-integer type casts (by source generator)

## Concept

It is difficult to handle enums in relation to non-integer types, but there is a classic workaround that makes handling them convenient by pseudo-handling them with structs.
EnumWithValues uses a source generator to make this workaround convenient and fast to use.

## Usage

Use `[EnumWithValues(structName)]` and `[EnumValue(params string[] values)]` attribute like below.

```csharp
[EnumWithValues("Foo", convertEnumValue: true, throwIfCastFails: false)]
public enum FooEnum {
    [EnumValue("OK")]
    OK = 200,
    [EnumValue("No Content")]
    No_Content = 204,
}
```

then source generator builds enum like `Foo` struct.

```csharp
Foo.No_Content == 204
```

### basic example

```csharp
using EnumWithValues;

namespace MyApp {
    [EnumWithValues("StatusCode")]
    public enum StatusCodeEnum {
        [EnumValue("OK")]
        OK = 200,
        [EnumValue("No Content")]
        No_Content = 204,
    }

    public class Response_StructBase {
        public StatusCode StatusCode { get; set; }
        public StatusCodeEnum StatusCodeEnum { get => StatusCode; set => StatusCode = value; }
    }

    public class Response_EnumBase {
        public StatusCode StatusCode { get => StatusCodeEnum; set => StatusCodeEnum = value; }
        public StatusCodeEnum StatusCodeEnum { get; set; }
    }

    [TestClass]
    public class MyAppTest {
        [TestMethod]
        public void StructBase() {
            var res = new Response_StructBase { StatusCode = 200 };
            Assert.IsTrue(res.StatusCode == res.StatusCodeEnum);
            Assert.IsTrue(res.StatusCode == "OK");
            Assert.IsTrue(res.StatusCode == 200);
            Assert.IsTrue(res.StatusCode == StatusCode.OK);
            Assert.IsTrue(res.StatusCode == StatusCodeEnum.OK);
        }

        [TestMethod]
        public void EnumBase() {
            var res = new Response_EnumBase { StatusCode = 200 };
            Assert.IsTrue(res.StatusCode == res.StatusCodeEnum);
            Assert.IsTrue(res.StatusCode == "OK");
            Assert.IsTrue(res.StatusCode == 200);
            Assert.IsTrue(res.StatusCode == StatusCode.OK);
            Assert.IsTrue(res.StatusCode == StatusCodeEnum.OK);
        }
    }
}

```

### with multiple values

```csharp
[EnumWithValues("Foo", convertEnumValue: false)] // do `convertEnumValue = false` when you use [EnumValue(int)]
public enum FooEnum {
    [EnumValue("A", 'a', 1)] // Foo.One == "A" && Foo.One == 'A' && Foo.One == 1
    One,
    [EnumValue("B", 'b', 2)] // MUST: same types order
    Two,
}
```

## Limitations

These limitations are caused by the fact that I don't know much about code scanning in Roslyn.

Pull Request welcome !!!

### enum values

EnumWithValues detects simple constant numeric value only.

```csharp
[EnumWithValues("Foo")]
public enum FooEnum : long {
    [EnumValue("OK")]
    OK = -200L, // OK
    [EnumValue("No Content")]
    No_Content = 200 + 4, // NG
}
```

### enum type

ulong enum value > long not supported.

## License

[Zlib License](LICENSE)
