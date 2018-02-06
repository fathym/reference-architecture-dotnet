using Fathym.Fabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.API.Actors
{
	public class WorkflowsHostingProgram
	{
		#region Properties
		public static WorkflowsHostingProgram Host = new WorkflowsHostingProgram();
		#endregion

		#region API Methods
		public virtual void Start<TActor>()
			where TActor : ActorBase
		{
			Start<TActor>((context, actorType) => new ActorService(context, actorType));
		}

		public virtual void StartWithAutoStart<TActor>()
			where TActor : ActorBase
		{
			Start<TActor>((context, actorType) => new AutoStartActorService(context, actorType));
		}

		public virtual void Start<TActor>(Func<StatefulServiceContext, ActorTypeInformation, ActorService> actorServiceFactory)
			where TActor : ActorBase
		{
			try
			{
				ActorRuntime.RegisterActorAsync<TActor>(actorServiceFactory).GetAwaiter().GetResult();

				//Thread.Sleep(Timeout.Infinite);
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
