using Fathym.Design;
using Fathym.Fabric.Communications;
using Fathym.Fabric.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net.Http;
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

		//public virtual TActor BuildPartitionClient<TActor>(string actorId, string applicationName = null, string serviceName = null)
		//	where TActor : IActor
		//{
		//	var client = ActorProxy.Create<TActor>(new ActorId(actorId),
		//		applicationName: applicationName,
		//		serviceName: serviceName);

		//	return client;
		//}

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

		public virtual async Task WithFabricClient(string application, string service, Func<HttpClient, Task> action)
		{
			var handler = loadHttpClientHandler(application, service);

			if (handler != null)
				await handler(action);
		}
		#endregion

		#region Helpers
		protected virtual ConfigurationSettings loadConfigurationSettings()
		{
			return context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;
		}

		protected abstract dynamic resolveServiceListener(Func<dynamic, ICommunicationListener> createCommunicationListener);

		#region Client Helpers
		protected virtual ICommunicationClientFactory<HttpCommunicationClient> loadCommunicationClient()
		{
			return new HttpCommunicationClientFactory(servicePartitionResolver: ServicePartitionResolver.GetDefault(),
				exceptionHandlers: new[] { new HttpClientExceptionHandler() });
		}

		protected virtual Func<Func<HttpClient, Task>, Task> loadFabricClientHandler(string application, string service)
		{
			var partitionClient = loadPartitionClient(application, service);

			Func<Func<HttpClient, Task>, Task> handler = async (h) =>
			{
				await partitionClient.InvokeWithRetryAsync(
					async (client) =>
					{
						using (client.HttpClient)
						{
							client.HttpClient.Timeout = TimeSpan.FromSeconds(60);

							await h(client.HttpClient);
						}
					});
			};

			return handler;
		}

		protected virtual Func<Func<HttpClient, Task>, Task> loadHttpClientHandler(string application, string service)
		{
			return DesignOutline.Instance.Chain<Func<Func<HttpClient, Task>, Task>>()
				.AddResponsibilities(
					() => loadFabricClientHandler(application, service))
				//() => loadDirectClientHandler(proxyContext))
				.Run().Result;
		}

		protected virtual ServicePartitionClient<HttpCommunicationClient> loadPartitionClient(string application,
			string service, long? partitionKey = null)
		{
			var clientFactory = loadCommunicationClient();

			if (partitionKey.HasValue)
				return new ServicePartitionClient<HttpCommunicationClient>(clientFactory,
					loadServiceUri(application, service), new ServicePartitionKey(partitionKey.Value));
			else
				return new ServicePartitionClient<HttpCommunicationClient>(clientFactory,
					loadServiceUri(application, service));
		}

		protected virtual Uri loadServiceUri(string application, string service)
		{
			return new Uri($@"fabric:/{application}/{service}");
		}
		#endregion
		#endregion
	}
}
