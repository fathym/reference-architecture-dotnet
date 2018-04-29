using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation.MVC.Fluent;
using Fathym.Presentation.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fathym.Presentation.MVC
{
	public class FathymStartup
	{
		#region Fields
		protected IConfigurationRoot config;

		protected readonly IHostingEnvironment env;

		protected readonly IFabricAdapter fabricAdapter;

		protected readonly ILoggerFactory loggerFactory;

		protected readonly IApplicationPipeline pipeline;
		#endregion

		#region Constructors
		public FathymStartup(IHostingEnvironment env, ILoggerFactory loggerFactory, IFabricAdapter fabricAdapter)
		{
			this.env = env;

			this.fabricAdapter = fabricAdapter;

			this.loggerFactory = loggerFactory;

			pipeline = buildApplicationPipeline();

			startupPipeline(pipeline.Startup());
		}
		#endregion

		#region API Methods
		public virtual void ConfigureServices(IServiceCollection services)
		{
			servicesPipeline(pipeline.Services(services));
		}

		public virtual void Configure(IApplicationBuilder app)
		{
			buildPipeline(pipeline.Build(app));
		}
		#endregion

		#region Helpers
		protected virtual IApplicationPipeline buildApplicationPipeline()
		{
			return new FathymApplicationPipeline(env, fabricAdapter, loggerFactory);
		}

		#region Builder Helpers
		protected virtual void buildPipeline(IApplicationBuilderPipeline pipeline)
		{
			setupCoreBuilder(pipeline.Core());

			setupProxyBuilder(pipeline.Proxy());

			setupViewBuilder(pipeline.View());
		}

		protected virtual void setupCoreBuilder(ICoreBuilderPipeline pipeline)
		{
			pipeline
				.Logging("Logging")
				.BrowerLink()
				.ExceptionHandling(null)
				.WWW()
				//.HTTPS()
				.Prerender()
				.Session()
				.Build(config, env, loggerFactory);
		}

		protected virtual void setupProxyBuilder(IProxyBuilderPipeline pipeline)
		{
			pipeline
				.AddQueryParamProcessor("Username", new UsernameQueryParamProcessor(new List<string>() { "username" }))
				.AddQueryParamProcessor("ClientIP", new ClientIPQueryParamProcessor(new List<string>() { "client-ip" }))
				.Build(config);
		}

		protected virtual void setupViewBuilder(IViewBuilderPipeline pipeline)
		{
			pipeline
				.StaticFiles(loadStaticFileOptions())
				.MVC(configureRoutes: registerDefaultRoutes)
				.Compression()
				.Build(config);
		}

		protected virtual IDictionary<string, string> loadFileExtensionContentTypes()
		{
			var types = new Dictionary<string, string>();
			types[".eot"] = "application/vnd.ms-fontobject";
			types[".ttf"] = "application/octet-stream";
			types[".svg"] = "image/svg+xml";
			types[".woff"] = "application/font-woff";
			types[".woff2"] = "application/font-woff2";
			types[".json"] = "application/json";

			return types;
		}

		protected virtual StaticFileOptions loadStaticFileOptions()
		{
			var contentTypes = loadFileExtensionContentTypes();

			var provider = new FileExtensionContentTypeProvider();

			contentTypes.ForEach(ct => provider.Mappings[ct.Key] = ct.Value);

			return new StaticFileOptions()
			{
				OnPrepareResponse = handleStaticFileGZipResponsePreparation,
				ContentTypeProvider = provider
			};
		}

		protected virtual void handleStaticFileGZipResponsePreparation(StaticFileResponseContext context)
		{
			var httpContext = context.Context;

			if (httpContext.Response.ContentType == "application/x-gzip")
			{
				if (context.File.Name.EndsWith("js.gz"))
					httpContext.Response.ContentType = "application/javascript";
				else if (context.File.Name.EndsWith("css.gz"))
					httpContext.Response.ContentType = "text/css";

				httpContext.Response.Headers.Add("Content-Encoding", new string[] { "gzip" });
			}
		}

		protected virtual void registerDefaultRoutes(IRouteBuilder routes)
		{
			routes.MapRoute(
				name: "default",
				template: "{*path}",
				defaults: new { controller = "Fathym", action = "Index" });
		}
		#endregion

		#region Services Helpers
		protected virtual void servicesPipeline(IApplicationServicesPipeline pipeline)
		{
			setupCoreServices(pipeline.Core());

			setupViewServices(pipeline.View());
		}

		protected virtual void setupCoreServices(ICoreServicesPipeline pipeline)
		{
			var dpConnConfig = loadDataProtectionConnectionConfig();

			var dpContConfig = loadDataProtectionContainerConfig();

			pipeline
				.Config()
				.Caching()
				.Compression()
				.DataProtection(dpConnConfig, dpContConfig)
				.Sessions(configureSessionOptions)
				.Set(config);
		}

		protected virtual void setupViewServices(IViewServicesPipeline pipeline)
		{
			pipeline
				.MVC(assemblies: new List<Assembly>()
				{
					Assembly.GetEntryAssembly()
				})
				.Proxy<FabricProxyService>()
				.Set(config);
		}

		protected virtual void configureSessionOptions(SessionOptions options)
		{
			options.IdleTimeout = TimeSpan.FromMinutes(30);

			options.Cookie.Name = $"{GetType().FullName}.Session";
		}

		protected virtual string loadDataProtectionConnectionConfig()
		{
			return "Data:Protection:Connection";
		}

		protected virtual string loadDataProtectionContainerConfig()
		{
			return "Data:Protection:Container";
		}
		#endregion

		#region Startup Helpers
		protected virtual void startupPipeline(IApplicationStartupPipeline pipeline)
		{
			config = setupConfiguration(pipeline.Configure());
		}

		protected virtual IConfigurationRoot setupConfiguration(IConfigurationBuilderPipeline pipeline)
		{
			return pipeline
				.AddConfig("appsettings.json", reloadOnChange: true)
				.AddConfig($"appsettings.{env.EnvironmentName}.json")
				.AddConfigGlob("*.config.json")
				.AddConfigGlob("configs/**/*.config.json")
				.AddConfigGlob("configs/**/*.json")
				.Build(env);
		}
		#endregion
		#endregion
	}
}
