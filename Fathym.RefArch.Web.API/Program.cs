using Fathym.Fabric.Hosting;
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
			Host.EstablishHostingPipeline()
				.Log(Host.GetType().Namespace)
				.Host(() =>
				{
					var serviceTypeName = "Fathym.RefArch.Web.APIType";

					Host.Start<Startup>(serviceTypeName, "ServiceEndpoint");

					return serviceTypeName;
				})
				.Run();
        }
    }
}
