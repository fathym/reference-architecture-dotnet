using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation.Compression;
using Fathym.Presentation.Prerender;
using Fathym.Presentation.Proxy;
using Fathym.Presentation.Rewrite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Fluent
{
	public class ApplicationBuilderPipeline : IBuilderPipelineStartup, IBuilderPipelineConfigure, IBuilderPipelineCloseout
	{
		#region Fields
		protected IApplicationBuilder app;

		protected IConfiguration config;

		protected readonly IHostingEnvironment env;

		protected readonly IFabricAdapter fabricAdapter;

		protected Func<bool> isDevelopment;

		protected readonly ILoggerFactory loggerFactory;
		#endregion

		#region Constructors
		public ApplicationBuilderPipeline(IApplicationBuilder app, IConfiguration config, IHostingEnvironment env, IFabricAdapter fabricAdapter, ILoggerFactory loggerFactory)
		{
			this.app = app;

			this.config = config;

			this.env = env;

			this.fabricAdapter = fabricAdapter;

			isDevelopment = () => env.IsDevelopment();

			this.loggerFactory = loggerFactory;
		}
		#endregion

		#region API Methods
		public virtual void Complete(string loggingConfigSectionName, string errorPagePath)
		{
			this
				.Startup(loggingConfigSectionName, errorPagePath)
				.ConfigureAll()
				.CloseoutAll();
		}

		#region Closeout
		public virtual void Closeout()
		{ }

		public virtual void CloseoutAll()
		{
			this
				.SetupMVC()
				.SetupCompression()
				.Closeout();
		}

		public virtual IBuilderPipelineCloseout SetupCompression()
		{
			//app.UseResponseCompression();
			app.UseCompression();

			return this;
		}

		public virtual IBuilderPipelineCloseout SetupMVC(Action<IRouteBuilder> configureRoutes = null)
		{
			app.UseMvc(configureRoutes ?? registerDefaultRoutes);

			return this;
		}

		public virtual IBuilderPipelineCloseout WithAppCloseout(Action<IApplicationBuilder> action)
		{
			action(app);

			return this;
		}
		#endregion

		#region Configure
		public virtual IBuilderPipelineCloseout Configure()
		{
			return this;
		}

		public virtual IBuilderPipelineCloseout ConfigureAll()
		{
			return this
				.SetupStaticFilesWithGzip()
				.Configure();
		}

		public virtual IBuilderPipelineConfigure SetupStaticFiles(StaticFileOptions options)
		{
			app.UseStaticFiles(options);

			return this;
		}

		public virtual IBuilderPipelineConfigure SetupStaticFilesWithGzip()
		{
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".eot"] = "application/vnd.ms-fontobject";
			provider.Mappings[".ttf"] = "application/octet-stream";
			provider.Mappings[".svg"] = "image/svg+xml";
			provider.Mappings[".woff"] = "application/font-woff";
			provider.Mappings[".woff2"] = "application/font-woff2";
			provider.Mappings[".json"] = "application/json";

			return SetupStaticFiles(new StaticFileOptions()
			{
				OnPrepareResponse = handleStaticFileGZipResponsePreparation,
				ContentTypeProvider = provider
			});
		}

		public virtual IBuilderPipelineConfigure WithAppConfigure(Action<IApplicationBuilder> action)
		{
			action(app);

			return this;
		}
		#endregion

		#region Startup
		public virtual IBuilderPipelineStartup SetIsDevelopmentCheck(Func<bool> check)
		{
			this.isDevelopment = check;

			return this;
		}

		public virtual IBuilderPipelineStartup SetupBrowserLink(bool forceSetup = false)
		{
			if (env.IsDevelopment() || forceSetup)
				app.UseBrowserLink();

			return this;
		}

		public virtual IBuilderPipelineStartup SetupExceptionHandling(string errorPagePath)
		{
			//	TODO:  Undo comment out once stable
			//if (isDevelopment())
			app.UseDeveloperExceptionPage();
			//else
			//	app.UseExceptionHandler(errorPagePath);

			return this;
		}

		public virtual IBuilderPipelineStartup SetupLogging(string configSectionName)
		{
			loggerFactory.AddConsole(config.GetSection("Logging"));
			loggerFactory.AddDebug();

			return this;
		}

		public virtual IBuilderPipelineStartup SetupPrerender()
		{
			//if (!isDevelopment())
			app.UsePrerender();

			return this;
		}

		public virtual IBuilderPipelineStartup SetupProxy()
		{
			app.UseProxy();

			return this;
		}

		public virtual IBuilderPipelineStartup SetupQueryParams(List<string> usernameQueryParams, List<string> clientIpQueryParams)
		{
			app.UseClientIPQueryParam(clientIpQueryParams);

			app.UseUsernameQueryParam(usernameQueryParams);

			return this;
		}

		public virtual IBuilderPipelineStartup SetupSessions()
		{
			app.UseSession();

			return this;
		}

		public virtual IBuilderPipelineStartup SetupRewrite(Func<RewriteOptions, RewriteOptions> action)
		{
			var options = new RewriteOptions();

			app.UseRewriter(action(options));

			return this;
		}

		public virtual IBuilderPipelineConfigure Startup()
		{
			return this;
		}

		public virtual IBuilderPipelineConfigure Startup(string loggingConfigSectionName, string errorPagePath)
		{
			return this
				.SetupLogging(loggingConfigSectionName)
				.SetupExceptionHandling(errorPagePath)
				.SetupBrowserLink()
				.SetupRewrite((options) =>
				{
					if (!env.IsDevelopment())
						return options
							.Add(new RedirectWWWRule());
					else
						return options;
					//.AddRedirectToHttps();  //  TODO:  Once ready with cert, add... write custom that can be configured by application
				})
				.SetupSessions()
				.SetupPrerender()
				.SetupQueryParams(new List<string> { "username" }, new List<string> { "clientIp" })
				.SetupProxy()
				.Startup();
		}

		public virtual IBuilderPipelineStartup WithAppStartup(Action<IApplicationBuilder> action)
		{
			action(app);

			return this;
		}
		#endregion
		#endregion

		#region Helpers
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
	}

	public interface IBuilderPipelineStartup
	{
		void Complete(string loggingConfigSectionName, string errorPagePath);

		IBuilderPipelineStartup SetIsDevelopmentCheck(Func<bool> check);

		IBuilderPipelineStartup SetupBrowserLink(bool forceSetup = false);

		IBuilderPipelineStartup SetupExceptionHandling(string errorPagePath);

		IBuilderPipelineStartup SetupLogging(string configSectionName);

		IBuilderPipelineStartup SetupPrerender();

		IBuilderPipelineStartup SetupProxy();

		IBuilderPipelineStartup SetupQueryParams(List<string> usernameQueryParams, List<string> clientIpQueryParams);

		IBuilderPipelineStartup SetupSessions();

		IBuilderPipelineStartup SetupRewrite(Func<RewriteOptions, RewriteOptions> action);

		IBuilderPipelineConfigure Startup();

		IBuilderPipelineConfigure Startup(string loggingConfigSectionName, string errorPagePath);

		IBuilderPipelineStartup WithAppStartup(Action<IApplicationBuilder> action);
	}

	public interface IBuilderPipelineConfigure
	{
		IBuilderPipelineCloseout Configure();

		IBuilderPipelineCloseout ConfigureAll();

		//	TODO:  IBuilderPipelineConfigure SetupAntiforgery();

		IBuilderPipelineConfigure SetupStaticFiles(StaticFileOptions options);

		IBuilderPipelineConfigure SetupStaticFilesWithGzip();

		IBuilderPipelineConfigure WithAppConfigure(Action<IApplicationBuilder> action);
	}

	public interface IBuilderPipelineCloseout
	{
		void Closeout();

		void CloseoutAll();

		IBuilderPipelineCloseout SetupCompression();

		IBuilderPipelineCloseout SetupMVC(Action<IRouteBuilder> configureRoutes = null);

		IBuilderPipelineCloseout WithAppCloseout(Action<IApplicationBuilder> action);
	}
}
