using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.RefArch.Workflows
{
    public interface IDoStuff : IActor
    {
		Task<string> Grab();
    }
}
