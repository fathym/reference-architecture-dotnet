using Fathym.Fabric.Runtime;
using Fathym.Fabric.Runtime.Host;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Text;
using System.Threading;

namespace Fathym.Fabric.Hosting
{
	public class StatelessServiceHostingProgram
	{
		#region Properties
		public static StatelessServiceHostingProgram Host = new StatelessServiceHostingProgram();
		#endregion

		#region API Methods
		public virtual void Start<TStartup>(string serviceTypeName, string serviceEndpointName)
			where TStartup : class
		{
			Start(serviceTypeName, context => new KestrelStatelessServiceHost<TStartup>(serviceEndpointName, context));
		}

		public virtual void Start(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory)
		{
			try
			{
				ServiceRuntime.RegisterServiceAsync(serviceTypeName, serviceFactory).GetAwaiter().GetResult();

				FabricEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);

				Thread.Sleep(Timeout.Infinite);
			}
			catch (Exception e)
			{
				FabricEventSource.Current.ServiceHostInitializationFailed(e.ToString());

				throw;
			}
		}
		#endregion
	}
}
