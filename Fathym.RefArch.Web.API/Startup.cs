using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation;
using Fathym.Presentation.MVC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fathym.RefArch.Web.API
{
    public class Startup : FathymAPIStartup
    {
		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory, IFabricAdapter fabricAdapter) 
			: base(env, loggerFactory, fabricAdapter)
		{ }
    }
}
