using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.RefArch.Workflows
{
    [StatePersistence(StatePersistence.None)]
	public class DoStuff : Actor, IDoStuff
	{
		public DoStuff(ActorService actorService, ActorId actorId)
					: base(actorService, actorId)
		{ }

		public Task<string> Grab()
		{
			return Task.FromResult("Hello world");
		}
	}
}
