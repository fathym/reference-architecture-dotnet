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
		protected virtual T loadConfigSetting<T>(string section, string name)
		{
			return fabricAdapter.GetConfiguration().LoadConfigSetting<T>(section, name);
		}

		protected virtual T loadConfigSetting<T>(string name)
		{
			return loadConfigSetting<T>(GetType().FullName, name);
		}
		
		protected virtual void setupLogging()
		{ }
		#endregion
	}
}
