﻿using System;
using Ninject;
using ProxyGenerator.NInject;
using ProxyNinjectDemonstration.ApplicationThings;
using ProxyNinjectDemonstration.ApplicationThings.Class1;
using ProxyNinjectDemonstration.ApplicationThings.Logger;
using ProxyNinjectDemonstration.ProxyRelated;

namespace ProxyNinjectDemonstration
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var kernel = new StandardKernel())
            {
                //bind application default things:

                kernel
                    .Bind<IConsoleLogger>()
                    .To<ConsoleLogger>()
                    .InSingletonScope()
                    ;

                //bind proxy module
                var proxyModule = new DemoProxyModule();
                kernel.Load(
                    proxyModule
                    );

                //bind application proxied things:

                kernel
                    .Bind<IInterface1ThatNeedToBeProxied>()
                    .ToProxy<IInterface1ThatNeedToBeProxied, Class1ThatNeedToBeProxied, ProxyAttribute>()
                    ;

                // --------------======= SHOW ME THE MAGIC =======--------------

                //proxied methods:
                var proxiedObject = kernel.Get<IInterface1ThatNeedToBeProxied>();

                proxiedObject.SumWithWait500Msec(1, 2);

                try
                {
                    proxiedObject.GenerateExceptionAfter250Msec();
                }
                catch
                {
                    //nothing to do in this mini demonstation
                }

                //not proxied methods:
                proxiedObject.GetCurrentDateTime_NotProxied();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
