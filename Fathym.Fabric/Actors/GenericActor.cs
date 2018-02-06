using Fathym.Design;
using Fathym.Fabric.Communications;
using Fathym.Fabric.Configuration;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Actors
{
	public class GenericActor : Actor
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;
		#endregion

		#region Constructors
		public GenericActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{
			DesignOutline.Instance.SetupCommonDefaultJSONSerialization();

			fabricAdapter = new StatefulFabricAdapter(actorService.Context);
		}
		#endregion

		#region Runtime
		protected override async Task OnActivateAsync()
		{
			setupLogging();

			FabricEventSource.Current.ServiceMessage(this, $"Activated {ActorService.Context.ServiceName}");

			await base.OnActivateAsync();
		}

		protected override async Task OnDeactivateAsync()
		{
			FabricEventSource.Current.ServiceMessage(this, $"Deactivated {ActorService.Context.ServiceName}");

			await base.OnDeactivateAsync();
		}
		#endregion

		#region Helpers
		protected virtual ICommunicationClientFactory<HttpCommunicationClient> loadCommunicationClient(string baseApiAddress)
		{
			return new HttpCommunicationClientFactory(baseApiAddress, servicePartitionResolver: ServicePartitionResolver.GetDefault(),
				exceptionHandlers: new[] { new HttpClientExceptionHandler() });
		}

		protected virtual T loadConfigSetting<T>(string section, string name)
		{
			return fabricAdapter.GetConfiguration().LoadConfigSetting<T>(section, name);
		}

		protected virtual T loadConfigSetting<T>(string name)
		{
			return fabricAdapter.GetConfiguration().LoadConfigSetting<T>(GetType().FullName, name);
		}
		
		protected virtual ServicePartitionClient<HttpCommunicationClient> loadPartitionClient(string application,
			string service, string baseApiAddress, long? partitionKey = null)
		{
			var clientFactory = loadCommunicationClient(baseApiAddress);

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

		protected virtual void setupLogging()
		{ }
		#endregion
	}
}
