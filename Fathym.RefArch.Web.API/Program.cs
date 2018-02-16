﻿using Fathym.Fabric.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.RefArch.Web.API
{
    public class Program : StatelessServiceHostingProgram
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
		{
			Host.Start<Startup>("Fathym.RefArch.Web.APIType", "ServiceEndpoint");

			Thread.Sleep(Timeout.Infinite);
        }
    }
}
