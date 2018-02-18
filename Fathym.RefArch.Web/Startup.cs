using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fathym.RefArch.Web
{
    public class Startup : FathymStartup
	{
		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory, IFabricAdapter fabricAdapter)
			: base(env, loggerFactory, fabricAdapter)
		{ }
    }
}
