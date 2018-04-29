using Fathym.Presentation.Fluent;
using Fathym.Presentation.Prerender;
using Fathym.Presentation.Proxy;
using Fathym.Presentation.Rewrite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Presentation.MVC.Fluent
{
	public class FathymApplicationBuilderPipeline : IApplicationBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected IConfigurationRoot config;

		protected IHostingEnvironment env;

		protected ILoggerFactory loggerFactory;
		#endregion

		#region Constructors
		public FathymApplicationBuilderPipeline(IApplicationBuilder app, IConfigurationRoot config, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			this.app = app;

			this.config = config;

			this.env = env;

			this.loggerFactory = loggerFactory;
		}
		#endregion

		#region API Methods
		public virtual ICoreBuilderPipeline Core()
		{
			return new FathymCoreBuilderPipeline(app, config, env, loggerFactory);
		}

		public virtual IProxyBuilderPipeline Proxy()
		{
			return new FathymProxyBuilderPipeline(app);
		}

		public virtual IViewBuilderPipeline View()
		{
			return new FathymViewBuilderPipeline(app);
		}
		#endregion
	}

	public interface IApplicationBuilderPipeline
	{
		ICoreBuilderPipeline Core();

		IProxyBuilderPipeline Proxy();

		IViewBuilderPipeline View();
	}

	#region Core
	public class FathymCoreBuilderPipeline : BaseOrderedPipeline, ICoreBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected IConfigurationRoot config;

		protected IHostingEnvironment env;

		protected string exceptionHandlingPage;

		protected ILoggerFactory loggerFactory;

		protected string loggingConfigSection;

		protected bool useHttps;

		protected bool useWww;
		#endregion

		#region Constructors
		public FathymCoreBuilderPipeline(IApplicationBuilder app, IConfigurationRoot config, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			this.app = app;

			this.config = config;

			this.env = env;

			this.loggerFactory = loggerFactory;
		}
		#endregion

		#region API Methods
		public virtual ICoreBuilderPipeline BrowerLink()
		{
			addAction(buildBrowserLink);

			return this;
		}

		public virtual void Build()
		{
			runActions();
		}

		public virtual ICoreBuilderPipeline ExceptionHandling(string errorPage)
		{
			exceptionHandlingPage = errorPage;

			addAction(buildExceptionHandling);

			return this;
		}

		public virtual ICoreBuilderPipeline Identity()
		{
			addAction(buildIdentity);

			return this;
		}

		public virtual ICoreBuilderPipeline Logging(string configSection = null)
		{
			loggingConfigSection = configSection;

			addAction(buildLogging);

			return this;
		}

		public virtual ICoreBuilderPipeline Prerender()
		{
			addAction(buildPrerender);

			return this;
		}

		public virtual ICoreBuilderPipeline Rewrite(bool useHttps = false, bool useWww = false)
		{
			this.useHttps = useHttps;

			this.useWww = useWww;

			if (useHttps || useWww)
				addAction(buildRewrite);

			return this;
		}

		public virtual ICoreBuilderPipeline Session()
		{
			addAction(buildSession);

			return this;
		}

		public virtual ICoreBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			addAction(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildBrowserLink()
		{
			if (env.IsDevelopment())
				app.UseBrowserLink();
		}

		protected virtual void buildExceptionHandling()
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else if (!exceptionHandlingPage.IsNullOrEmpty())
				app.UseExceptionHandler(exceptionHandlingPage);
		}

		protected virtual void buildIdentity()
		{
			app.UseAuthentication();
		}

		protected virtual void buildLogging()
		{
			if (!loggingConfigSection.IsNullOrEmpty())
			{
				var section = config.GetSection(loggingConfigSection);

				if (section == null)
					loggerFactory.AddConsole(section);
			}

			loggerFactory.AddDebug();
		}

		protected virtual void buildPrerender()
		{
			if (!env.IsDevelopment())
				app.UsePrerender();
		}

		protected virtual void buildRewrite()
		{
			var options = new RewriteOptions();

			if (!env.IsDevelopment())
			{
				if (useWww)
					options = options.Add(new RedirectWWWRule());

				if (useHttps)
					options = options.AddRedirectToHttps();
			}

			app.UseRewriter(options);
		}

		protected virtual void buildSession()
		{
			app.UseSession();
		}
		#endregion
	}

	public interface ICoreBuilderPipeline
	{
		ICoreBuilderPipeline BrowerLink();

		ICoreBuilderPipeline ExceptionHandling(string errorPage);

		ICoreBuilderPipeline Identity();

		ICoreBuilderPipeline Logging(string configSection = null);

		ICoreBuilderPipeline Prerender();

		ICoreBuilderPipeline Rewrite(bool useHttps = false, bool useWww = false);

		ICoreBuilderPipeline Session();

		ICoreBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build();
	}
	#endregion

	#region Proxy
	public class FathymProxyBuilderPipeline : BaseOrderedPipeline, IProxyBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected IDictionary<string, IQueryParamProcessor> queryParamProcessors;
		#endregion

		#region Constructors
		public FathymProxyBuilderPipeline(IApplicationBuilder app)
		{
			this.app = app;

			queryParamProcessors = new Dictionary<string, IQueryParamProcessor>();
		}
		#endregion

		#region API Methods
		protected virtual void setupCoreBuilder(ICoreBuilderPipeline pipeline)
		{
			pipeline
				.Logging("Logging")
				.BrowerLink()
				.ExceptionHandling(null)
				.Rewrite(useWww: true/*, useHttps: true*/)
				.Prerender()
				.Session()
				.Build();
		}

		public virtual IProxyBuilderPipeline AddQueryParamProcessor(string name, IQueryParamProcessor queryParamProcessor)
		{
			queryParamProcessors[name] = queryParamProcessor;

			return this;
		}

		public virtual void Build()
		{
			runActions();
		}

		public virtual IProxyBuilderPipeline Proxy()
		{
			addAction(buildProxy);

			return this;
		}

		public virtual IProxyBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			addAction(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildProxy()
		{
			app.UseProxy(queryParamProcessors);
		}
		#endregion
	}

	public interface IProxyBuilderPipeline
	{
		IProxyBuilderPipeline AddQueryParamProcessor(string name, IQueryParamProcessor queryParamProcessor);

		IProxyBuilderPipeline Proxy();

		IProxyBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build();
	}
	#endregion

	#region View
	public class FathymViewBuilderPipeline : BaseOrderedPipeline, IViewBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected Action<IRouteBuilder> mvcConfigureRoutes;

		protected bool mvcUseDefaultRoutes;

		protected StaticFileOptions staticFileOptions;
		#endregion

		#region Constructors
		public FathymViewBuilderPipeline(IApplicationBuilder app)
		{
			this.app = app;
		}
		#endregion

		#region API Methods
		public virtual void Build()
		{
			runActions();
		}

		public virtual IViewBuilderPipeline Compression()
		{
			addAction(buildCompression);

			return this;
		}

		public virtual IViewBuilderPipeline MVC(Action<IRouteBuilder> configureRoutes = null, bool useDefaultRoutes = false)
		{
			mvcConfigureRoutes = configureRoutes;

			mvcUseDefaultRoutes = useDefaultRoutes;

			addAction(buildMVC);

			return this;
		}

		public virtual IViewBuilderPipeline StaticFiles(StaticFileOptions options)
		{
			staticFileOptions = options;

			addAction(buildStaticFiles);

			return this;
		}

		public virtual IViewBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			addAction(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildCompression()
		{
			app.UseResponseCompression();
		}

		protected virtual void buildMVC()
		{
			if (mvcConfigureRoutes != null)
				app.UseMvc(mvcConfigureRoutes);
			else if (mvcUseDefaultRoutes)
				app.UseMvcWithDefaultRoute();
			else
				app.UseMvc();
		}

		protected virtual void buildStaticFiles()
		{
			if (staticFileOptions != null)
				app.UseStaticFiles(staticFileOptions);
			else
				app.UseStaticFiles();
		}
		#endregion
	}

	public interface IViewBuilderPipeline
	{
		IViewBuilderPipeline Compression();

		IViewBuilderPipeline MVC(Action<IRouteBuilder> configureRoutes = null, bool useDefaultRoutes = false);

		IViewBuilderPipeline StaticFiles(StaticFileOptions options);

		IViewBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build();
	}
	#endregion
}
