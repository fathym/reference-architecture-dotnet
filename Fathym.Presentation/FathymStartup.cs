using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation.Fluent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation
{
	public abstract class FathymStartup
	{
		#region Fields
		protected IConfigurationRoot config;

		protected readonly IHostingEnvironment env;

		protected readonly IFabricAdapter fabricAdapter;

		protected readonly ILoggerFactory loggerFactory;
		#endregion

		#region Constructors
		public FathymStartup(IHostingEnvironment env, ILoggerFactory loggerFactory, IFabricAdapter fabricAdapter)
		{
			this.env = env;

			this.fabricAdapter = fabricAdapter;

			this.loggerFactory = loggerFactory;

			config = buildConfigurationRoot();
		}
		#endregion

		#region API Methods
		public virtual IServiceProvider ConfigureServices(IServiceCollection services)
		{
			var pipeline = setupAppServicesPipeline(services);

			return configureAppServicesPipeline(pipeline);
		}

		public virtual void Configure(IApplicationBuilder a)
		{
			var pipeline = setupAppBuilderPipeline(a);

			configureAppBuilderPipeline(pipeline);
		}
		#endregion

		#region Helpers
		protected virtual void addJsonConfigs(IConfigurationBuilder builder)
		{
			var configs = new Matcher();

			configs.AddInclude("*.config.json");

			configs.AddInclude("configs/**/*.config.json");

			configs.AddInclude("configs/**/*.json");

			var results = configs.Execute(new DirectoryInfoWrapper(new DirectoryInfo(env.ContentRootPath)));

			if (!results.Files.IsNullOrEmpty())
				results.Files.ForEach(file => builder.AddJsonFile(file.Path));
		}

		protected virtual IConfigurationRoot buildConfigurationRoot()
		{
			var builder = loadConfigurationBuilder();

			return builder.Build();
		}

		protected virtual void configureAppBuilderPipeline(IBuilderPipelineStartup pipeline)
		{
			pipeline
				.Startup("Logging", "/Home/Error")
				.ConfigureAll()
				.CloseoutAll();
		}


		protected virtual IServiceProvider configureAppServicesPipeline(IServicesPipelineStartup pipeline)
		{
			return pipeline
				.ConfigureAll($"{GetType().Namespace}.Session");
		}

		protected virtual IConfigurationBuilder loadConfigurationBuilder()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			addJsonConfigs(builder);

			builder.AddEnvironmentVariables();

			return builder;
		}

		protected virtual IBuilderPipelineStartup setupAppBuilderPipeline(IApplicationBuilder app)
		{
			var pipeline = new ApplicationBuilderPipeline(app, config, env, fabricAdapter, loggerFactory);

			return pipeline;
		}

		protected virtual IServicesPipelineStartup setupAppServicesPipeline(IServiceCollection services)
		{
			var pipeline = new ApplicationServicesPipeline(services, config, env, fabricAdapter, loggerFactory);

			return pipeline;
		}
		#endregion
	}
}
