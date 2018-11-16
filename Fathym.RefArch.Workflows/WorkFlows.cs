using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Fathym.RefArch.Workflows.Interfaces;
using Fathym.RefArch.Workflows;
using Fathym.Fabric.Actors;
using Fathym.API;

namespace Fathym.RefArch
{
    internal class WorkFlows : GenericActor, IWorkService
    {
        public WorkFlows(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        { }

        #region Runtime
        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }
		#endregion

		#region API Methods
		public override async Task Refresh()
		{ }
		#endregion

		#region API Methods
		public virtual async Task<BaseResponse> Ping()
        {
            return new BaseResponse()
            {
                Status = Status.Success
            };
        }
        #endregion
    }
}
