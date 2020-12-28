using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var res = new Response_EnumBase { StatusCode = 204 };
            Assert.IsTrue(res.StatusCode == res.StatusCodeEnum);
            Assert.IsTrue(res.StatusCode == "No Content");
            Assert.IsTrue(res.StatusCode == 204);
            Assert.IsTrue(res.StatusCode == StatusCode.No_Content);
            Assert.IsTrue(res.StatusCode == StatusCodeEnum.No_Content);
        }
    }
}

