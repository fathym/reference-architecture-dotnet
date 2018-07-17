using Fathym.Fabric.Runtime.Adapters;
using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Fabric.Actors
{
	public abstract class ActorFactory<TWorkflow>
		where TWorkflow : IActor
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;
		#endregion

		#region Constructors
		public ActorFactory(IFabricAdapter fabricAdapter)
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

			return fabricAdapter.BuildActorProxy<TWorkflow>(actorId, applicationName: getApplicationName(), serviceName: getServiceName());
		}
		#endregion

		#region Helpers
		protected abstract string buildActorId(string primaryApiKey);

		protected virtual string getApplicationName()
		{
			return null;
		}

		protected virtual string getServiceName()
		{
			return null;
		}
		#endregion
	}
}
