﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.Parameters
{
    /// <summary>
    /// Summary description for GenericFixture
    /// </summary>
    [TestClass]
    public class GenericParameter1Fixture
    {
        public GenericParameter1Fixture()
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
        public void TestGeneric0()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock14, ClassMock14>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action<int>((int x) =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X1(new Dictionary<string, int>());

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public interface IClassMock14
        {
            [TestWrapWithProxy]
            void X1(Dictionary<string, int> x);

            void X2();
        }

        public class ClassMock14 : IClassMock14
        {
            private Action<int> _ok1;
            private Action _ok2;

            public ClassMock14(Action<int> ok1)
            {
                _ok1 = ok1;
            }

            public ClassMock14(Action<int> ok1, Action ok2)
            {
                _ok1 = ok1;
                _ok2 = ok2;
            }

            #region Implementation of IClassMock14

            public void X1(Dictionary<string, int> x)
            {
                _ok1(x.Count);
            }

            public void X2()
            {
                if (_ok2 != null)
                {
                    _ok2();
                }
            }

            #endregion
        }

    }

}
