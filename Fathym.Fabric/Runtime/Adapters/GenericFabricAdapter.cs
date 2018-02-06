using Fathym.Fabric.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Runtime.Adapters
{
	public abstract class GenericFabricAdapter<TContext> : IFabricAdapter
		where TContext : ServiceContext
	{
		#region Fields
		protected readonly TContext context;

		protected readonly IDictionary<Type, object> services;
		#endregion

		#region Constructors
		public GenericFabricAdapter(TContext context)
		{
			this.context = context;

			services = new Dictionary<Type, object>();
		}
		#endregion

		#region API Methods
		public virtual TActor BuildActorProxy<TActor>(string actorId, string applicationName = null, string serviceName = null)
			where TActor : IActor
		{
			var client = ActorProxy.Create<TActor>(new ActorId(actorId), 
				applicationName: applicationName,
				serviceName: serviceName);

			return client;
		}

		public virtual dynamic BuildServiceListener(string listenerName, Func<string, ServiceContext, ICommunicationListener> listener)
		{
			return resolveServiceListener(initParams => listener(listenerName, initParams));
		}

		public virtual IConfigurationManager GetConfiguration()
		{
			return new FabricConfigurationManager(loadConfigurationSettings());
		}

		public virtual RuntimeContext GetContext()
		{
			return new RuntimeContext()
			{
				ApplicationName = context.CodePackageActivationContext.ApplicationTypeName.Replace("Type", String.Empty),
				ServiceName = context.ServiceTypeName.Replace("Type", String.Empty)
			};
		}
		#endregion

		#region Helpers
		protected virtual ConfigurationSettings loadConfigurationSettings()
		{
			return context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;
		}

		protected abstract dynamic resolveServiceListener(Func<dynamic, ICommunicationListener> createCommunicationListener);
		#endregion
	}
}
