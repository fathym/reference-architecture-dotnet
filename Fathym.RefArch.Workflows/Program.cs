using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Fathym.Fabric.API.Workflows;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Fathym.RefArch.Workflows
{
    public class Program : WorkflowsHostingProgram
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
					var serviceTypeName = $"{Host.GetType().Namespace}Type";

					Host.Start<Workflows>();

					return serviceTypeName;
				})
				.Run();
        }
    }
}
