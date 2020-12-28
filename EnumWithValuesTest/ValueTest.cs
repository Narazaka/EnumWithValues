using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnumWithValues;

namespace ValueTestNS {
    [EnumWithValues("DefaultValueTest")]
    enum DefaultValueTestEnum {
        [EnumValue("FOO", 1)]
        Foo,
        [EnumValue("BAR", 2)]
        Bar,
        Baz,
    }

    [EnumWithValues("WithValueTest", true)]
    enum WithValueTestEnum {
        [EnumValue("FOO")]
        Foo,
        [EnumValue("BAR")]
        Bar,
    }

    [EnumWithValues("WithValueSpecifiedTest", true)]
    enum WithValueSpecifiedTestEnum {
        [EnumValue("FOO")]
        Foo = -200,
        [EnumValue("BAR")]
        Bar = 200,
        [EnumValue("BAZ")]
        Baz,
    }

    [EnumWithValues("WithTypedValueTest", true)]
    enum WithTypedValueTestEnum : long {
        [EnumValue("FOO")]
        Foo = -200L,
        [EnumValue("BAR")]
        Bar,
    }

    [EnumWithValues("WithSystemTypedValueTest", true)]
    enum WithSystemTypedValueTestEnum : System.Byte {
        [EnumValue("FOO")]
        Foo = 1,
        [EnumValue("BAR")]
        Bar,
    }

    [TestClass]
    public class ValueTest {
        [TestMethod]
        public void Default() {
            Assert.IsTrue(DefaultValueTest.Foo == DefaultValueTestEnum.Foo);
            Assert.IsTrue(DefaultValueTest.Foo == "FOO");
            Assert.IsTrue(DefaultValueTest.Foo == 1);
            Assert.IsTrue(DefaultValueTest.Bar == 2);
        }

        [TestMethod]
        public void InvalidCastDefault() {
            Assert.IsTrue(DefaultValueTest.Foo == 0);
            Assert.IsTrue(DefaultValueTest.Bar != 0);
            Assert.IsTrue(DefaultValueTest.Foo == "");
            Assert.IsTrue(DefaultValueTest.Bar != "");
            Assert.IsTrue(0 == ((DefaultValueTest)0).Enum);
            Assert.IsTrue(DefaultValueTest.Foo == DefaultValueTestEnum.Baz);
            Assert.IsTrue(DefaultValueTest.Bar != DefaultValueTestEnum.Baz);
        }

        [TestMethod]
        public void WithValue() {
            Assert.IsTrue(WithValueTest.Foo == WithValueTestEnum.Foo);
            Assert.IsTrue(WithValueTest.Foo == "FOO");
            Assert.IsTrue(WithValueTest.Foo == 0);
            Assert.IsTrue(WithValueTest.Bar == 1);
        }

        [TestMethod]
        public void WithValueSpecified() {
            Assert.IsTrue(WithValueSpecifiedTest.Foo == WithValueSpecifiedTestEnum.Foo);
            Assert.IsTrue(WithValueSpecifiedTest.Foo == "FOO");
            Assert.IsTrue(WithValueSpecifiedTest.Foo == -200);
            Assert.IsTrue(WithValueSpecifiedTest.Bar == 200);
            Assert.IsTrue(WithValueSpecifiedTest.Baz == 201);
        }

        [TestMethod]
        public void WithTypedValue() {
            Assert.IsTrue(WithTypedValueTest.Foo == WithTypedValueTestEnum.Foo);
            Assert.IsTrue(WithTypedValueTest.Foo == "FOO");
            Assert.IsTrue(WithTypedValueTest.Foo == -200);
            Assert.IsTrue(WithTypedValueTest.Bar == -199);
        }

        [TestMethod]
        public void WithSystemTypedValue() {
            Assert.IsTrue(WithSystemTypedValueTest.Foo == WithSystemTypedValueTestEnum.Foo);
            Assert.IsTrue(WithSystemTypedValueTest.Foo == "FOO");
            Assert.IsTrue(WithSystemTypedValueTest.Foo == 1);
            Assert.IsTrue(WithSystemTypedValueTest.Bar == 2);
        }
    }
}

