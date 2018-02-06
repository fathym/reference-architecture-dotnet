using Fathym.Fabric.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Runtime.Adapters
{
	public interface IFabricAdapter
	{
		TActor BuildActorProxy<TActor>(string actorId, string applicationName = null, string serviceName = null) where TActor : IActor;

		dynamic BuildServiceListener(string listenerName, Func<string, ServiceContext, ICommunicationListener> listener);

		RuntimeContext GetContext();

		IConfigurationManager GetConfiguration();
	}
}
