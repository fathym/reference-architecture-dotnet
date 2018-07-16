using Fathym.Fabric.Runtime.Adapters;
using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Fabric.API.Workflows
{
	public abstract class DomainWorkflowFactory<TWorkflow>
		where TWorkflow : DomainWorkflowActor, IActor
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;
		#endregion

		#region Constructors
		public DomainWorkflowFactory(IFabricAdapter fabricAdapter)
		{
			this.fabricAdapter = fabricAdapter;
		}
		#endregion

		#region API Methods
		public virtual TWorkflow Build(string primaryApiKey, bool makeUnique = false)
		{
			var actorId = buildActorId(primaryApiKey);

			if (makeUnique)
				actorId += $"|{Guid.NewGuid()}";

			return fabricAdapter.BuildActorProxy<TWorkflow>(actorId);
		}
		#endregion

		#region Helpers
		protected abstract string buildActorId(string primaryApiKey);
		#endregion
	}
}
