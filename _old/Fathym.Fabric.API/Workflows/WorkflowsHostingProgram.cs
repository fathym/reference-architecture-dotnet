using Fathym.Fabric.Actors;
using Fathym.Fabric.Hosting;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.API.Workflows
{
	public class WorkflowsHostingProgram : BaseHostingProgram<WorkflowsHostingProgram>
	{
		#region Properties
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
			ActorRuntime.RegisterActorAsync<TActor>(actorServiceFactory).GetAwaiter().GetResult();
		}
		#endregion

		#region API Methods

		#endregion
	}
}
