using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnumWithValues;

namespace Ns1 {
    [EnumWithValues("NamespacedMyEnumStruct")]
    enum NamespacedMyEnumNs {
        [EnumValue("FOO")]
        Foo,
        [EnumValue("BAR")]
        Bar,
    }

    [TestClass]
    public class NamespaceTest {
        [TestMethod]
        public void Initialize() {
            Assert.IsTrue(NamespacedMyEnumStruct.Foo == "FOO");
            Assert.AreEqual(NamespacedMyEnumStruct.Bar.AsEnum, NamespacedMyEnumNs.Bar);
        }
    }
}

