using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnumWithValues;

[EnumWithValues("EmptyMyEnumStruct")]
enum EmptyMyEnum {

}

[TestClass]
public class EmptyTest {
    [TestMethod]
    public void Main() {
        Assert.IsInstanceOfType(new EmptyMyEnum(), typeof(EmptyMyEnum));
    }
}

