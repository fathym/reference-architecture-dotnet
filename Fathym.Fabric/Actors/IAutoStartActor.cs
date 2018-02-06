using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Actors
{
	public interface IAutoStartActor : IActor
	{
		Task<Status> AutoStart();
	}
}
