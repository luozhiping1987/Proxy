﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.PerformanceContainer
{
    /// <summary>
    /// Summary description for UnitTest4
    /// </summary>
    [TestClass]
    public class UnitTest4
    {
        public UnitTest4()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestDisposePerformanceSession0()
        {
            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateRecord(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Mock<IPerformanceRecord>().Object);
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Returns(new Mock<IPerformanceRecord>().Object);

            var sf = new Mock<IPerformanceSaver>();

            var pc = new PerformanceTelemetry.Container.PerformanceContainer(
                new EmptyLoggerAdapter(),
                rf.Object,
                sf.Object);

            pc.Dispose();

            sf.Verify(j => j.Dispose(), Times.AtLeastOnce());
        }

    }
}
