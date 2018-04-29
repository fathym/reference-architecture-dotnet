using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation;
using Fathym.Presentation.MVC;
using Fathym.Presentation.MVC.Fluent;
using Fathym.Presentation.Proxy;
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

		public override void Configure(IApplicationBuilder app)
		{
			app.Use(async (context, next) =>
			{
				await context.HandleContext(ProxyContext.Lookup,
					async (proxyContext) =>
					{
						proxyContext.Proxy.Connection = new ProxyConnection()
						{
							Application = "Fathym.RefArch.Fabric",
							Service = "Fathym.RefArch.Web.API"
						};
					},
					create: async () => new ProxyContext() { Proxy = new ProxySetup() });

				await next();
			});

			base.Configure(app);
		}
	}
}
