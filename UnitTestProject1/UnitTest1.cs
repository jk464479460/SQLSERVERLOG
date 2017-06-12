using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSERVERLOG;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IDb db = new Db();
            var dt=db.GetLogByTable("t7");
        }
    }
}
