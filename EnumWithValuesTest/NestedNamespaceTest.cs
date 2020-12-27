using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnumWithValues;

namespace Nested.NS {
    [EnumWithValues("NestedMyEnumStruct")]
    enum NestedMyEnum {
        [EnumValue("FOO")]
        Foo,
        [EnumValue("BAR")]
        Bar,
    }

    [TestClass]
    public class NestedNamespaceTest {
        [TestMethod]
        public void Initialize() {
            Assert.IsTrue(NestedMyEnumStruct.Foo == "FOO");
        }
    }
}

