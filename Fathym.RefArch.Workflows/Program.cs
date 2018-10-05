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
        public static void Main()
		{
			Host.EstablishHostingPipeline()
				.Log(typeof(Program).Namespace)
				.Host(() =>
				{
					var serviceTypeName = $"{typeof(WorkFlows).FullName}Type";

					Host.Start<WorkFlows>();

					return serviceTypeName;
				})
				.Run();
        }
    }
}
