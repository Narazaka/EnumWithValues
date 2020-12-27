using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnumWithValues;

[EnumWithValues("ToplevelMyEnumStruct")]
enum ToplevelMyEnum {
    [EnumValue("FOO")]
    Foo,
    [EnumValue("BAR")]
    Bar,
}

[TestClass]
public class ToplevelTest {
    [TestMethod]
    public void Main() {
        Assert.IsTrue(ToplevelMyEnumStruct.Bar == "BAR");
    }
}

