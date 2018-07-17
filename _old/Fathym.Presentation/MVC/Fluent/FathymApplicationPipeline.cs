using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Fathym.Presentation.Proxy;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Rewrite;
using Fathym.Presentation.Rewrite;
using Fathym.Presentation.Prerender;
using Microsoft.AspNetCore.Routing;

namespace Fathym.Presentation.MVC.Fluent
{
	public class FathymApplicationPipeline : IApplicationPipeline
	{
		#region Fields
		protected IApplicationBuilder app;

		protected readonly IHostingEnvironment env;

		protected readonly IFabricAdapter fabricAdapter;

		protected Func<bool> isDevelopment;

		protected readonly ILoggerFactory loggerFactory;
		#endregion

		#region Constructors
		public FathymApplicationPipeline(IHostingEnvironment env, IFabricAdapter fabricAdapter, ILoggerFactory loggerFactory)
		{
			this.env = env;

			this.fabricAdapter = fabricAdapter;

			isDevelopment = () => env.IsDevelopment();

			this.loggerFactory = loggerFactory;
		}
		#endregion

		#region API Methods
		public IApplicationBuilderPipeline Build(IApplicationBuilder app, IConfigurationRoot config)
		{
			return new FathymApplicationBuilderPipeline(app, config, env, loggerFactory);
		}

		public IApplicationServicesPipeline Services(IServiceCollection services, IConfigurationRoot config)
		{
			return new FathymApplicationServicesPipeline(services, config);
		}

		public IApplicationStartupPipeline Startup()
		{
			return new FathymApplicationStartupPipeline();
		}
		#endregion

		#region Helpers

		#endregion
	}

	public interface IApplicationPipeline
	{
		IApplicationBuilderPipeline Build(IApplicationBuilder app, IConfigurationRoot config);

		IApplicationServicesPipeline Services(IServiceCollection services, IConfigurationRoot config);

		IApplicationStartupPipeline Startup();
	}
}
