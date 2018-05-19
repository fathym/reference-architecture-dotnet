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
        private static void Main()
		{
			Host.EstablishHostingPipeline()
				//.Log(Host.GetType().FullName)
				.Host(() =>
				{
					Host.Start<Workflows>();

					return $"{Host.GetType().FullName}Type";
				})
				.Run();
        }
    }
}
