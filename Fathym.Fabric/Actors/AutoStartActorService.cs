using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Fabric.Actors
{
	public class AutoStartActorService : ActorService
	{
		#region Fields
		#endregion

		#region Constructors
		public AutoStartActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo,
			Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
			IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
			: base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
		{
		}
		#endregion

		#region Runtime
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			await base.RunAsync(cancellationToken);

			var appName = Context.CodePackageActivationContext.ApplicationName;

			var serviceName = ActorTypeInformation.ServiceName;

			var autoStartKey = $"{appName}.{serviceName}.AutoStart";

			FabricEventSource.Current.Message($"Auto Starting {autoStartKey}");

			var proxy = ActorProxy.Create<IAutoStartActor>(new ActorId(autoStartKey),
				applicationName: appName,
				serviceName: serviceName);

			var status = await proxy.AutoStart();

			if (!status)
				throw new Exception(status.Message);

			FabricEventSource.Current.Message($"Auto Start Complete for {autoStartKey}");
		}
		#endregion
	}
}
