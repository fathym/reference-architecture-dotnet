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
		#endregion

		#region Constructors
		public FathymApplicationBuilderPipeline(IApplicationBuilder app)
		{
			this.app = app;
		}
		#endregion

		#region API Methods
		public virtual ICoreBuilderPipeline Core()
		{
			return new FathymCoreBuilderPipeline(app);
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
	public class FathymCoreBuilderPipeline : ICoreBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected List<Action> appActions;

		protected string exceptionHandlingPage;

		protected string loggingConfigSection;

		protected bool useBrowserLink;

		protected bool useHTTPS;

		protected bool useIdentity;

		protected bool useLogging;

		protected bool usePrerender;

		protected bool useSession;

		protected bool useWWW;
		#endregion

		#region Constructors
		public FathymCoreBuilderPipeline(IApplicationBuilder app)
		{
			this.app = app;

			appActions = new List<Action>();
		}
		#endregion

		#region API Methods
		public virtual ICoreBuilderPipeline BrowerLink()
		{
			useBrowserLink = true;

			return this;
		}

		public virtual void Build(IConfigurationRoot config, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (useBrowserLink)
				buildBrowserLink(config, env);

			if (!exceptionHandlingPage.IsNullOrEmpty())
				buildExceptionHandling(config, env);

			if (useIdentity)
				buildIdentity(config, env);

			if (useLogging)
				buildLogging(config, env, loggerFactory);

			if (usePrerender)
				buildPrerender(config, env);

			if (useHTTPS || useWWW)
				buildRewrite(config, env);

			if (useSession)
				buildSession(config, env);
		}

		public virtual ICoreBuilderPipeline ExceptionHandling(string errorPage)
		{
			exceptionHandlingPage = errorPage;

			return this;
		}

		public virtual ICoreBuilderPipeline HTTPS()
		{
			useHTTPS = true;

			return this;
		}

		public virtual ICoreBuilderPipeline Identity()
		{
			useIdentity = true;

			return this;
		}

		public virtual ICoreBuilderPipeline Logging(string configSection = null)
		{
			useLogging = true;

			loggingConfigSection = configSection;

			return this;
		}

		public virtual ICoreBuilderPipeline Prerender()
		{
			usePrerender = true;

			return this;
		}

		public virtual ICoreBuilderPipeline Session()
		{
			useSession = true;

			return this;
		}

		public virtual ICoreBuilderPipeline WWW()
		{
			useWWW = true;

			return this;
		}

		public virtual ICoreBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			appActions.Add(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildActions(IConfigurationRoot config, IHostingEnvironment env)
		{
			appActions.ForEach(action => action());

			appActions.Clear();
		}

		protected virtual void buildBrowserLink(IConfigurationRoot config, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseBrowserLink();
		}

		protected virtual void buildExceptionHandling(IConfigurationRoot config, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseExceptionHandler(exceptionHandlingPage);
		}

		protected virtual void buildIdentity(IConfigurationRoot config, IHostingEnvironment env)
		{
			app.UseAuthentication();
		}

		protected virtual void buildLogging(IConfigurationRoot config, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (!loggingConfigSection.IsNullOrEmpty())
			{
				var section = config.GetSection(loggingConfigSection);

				if (section == null)
					loggerFactory.AddConsole(section);
			}

			loggerFactory.AddDebug();
		}

		protected virtual void buildPrerender(IConfigurationRoot config, IHostingEnvironment env)
		{
			if (!env.IsDevelopment())
				app.UsePrerender();
		}

		protected virtual void buildRewrite(IConfigurationRoot config, IHostingEnvironment env)
		{
			var options = new RewriteOptions();

			if (!env.IsDevelopment())
			{
				if (useWWW)
					options = options.Add(new RedirectWWWRule());

				if (useHTTPS)
					options = options.AddRedirectToHttps();
			}

			app.UseRewriter(options);
		}

		protected virtual void buildSession(IConfigurationRoot config, IHostingEnvironment env)
		{
			app.UseSession();
		}
		#endregion
	}

	public interface ICoreBuilderPipeline
	{
		ICoreBuilderPipeline BrowerLink();

		ICoreBuilderPipeline ExceptionHandling(string errorPage);

		ICoreBuilderPipeline HTTPS();

		ICoreBuilderPipeline Logging(string configSection = null);

		ICoreBuilderPipeline Prerender();

		ICoreBuilderPipeline Session();

		ICoreBuilderPipeline WWW();

		ICoreBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build(IConfigurationRoot config, IHostingEnvironment env, ILoggerFactory loggerFactory);
	}
	#endregion

	#region Proxy
	public class FathymProxyBuilderPipeline : IProxyBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected List<Action> appActions;

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
		public virtual IProxyBuilderPipeline AddQueryParamProcessor(string name, IQueryParamProcessor queryParamProcessor)
		{
			queryParamProcessors[name] = queryParamProcessor;

			return this;
		}

		public virtual void Build(IConfigurationRoot config)
		{
			if (!queryParamProcessors.IsNullOrEmpty())
				buildQueryParamProcessors(config);
		}


		public virtual IProxyBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			appActions.Add(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildActions(IConfigurationRoot config, IHostingEnvironment env)
		{
			appActions.ForEach(action => action());

			appActions.Clear();
		}

		protected virtual void buildQueryParamProcessors(IConfigurationRoot config)
		{
			app.UseProxy(queryParamProcessors);
		}
		#endregion
	}

	public interface IProxyBuilderPipeline
	{
		IProxyBuilderPipeline AddQueryParamProcessor(string name, IQueryParamProcessor queryParamProcessor);

		IProxyBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build(IConfigurationRoot config);
	}
	#endregion

	#region View
	public class FathymViewBuilderPipeline : IViewBuilderPipeline
	{
		#region Fields
		protected readonly IApplicationBuilder app;

		protected List<Action> appActions;

		protected Action<IRouteBuilder> mvcConfigureRoutes;

		protected bool mvcUseDefaultRoutes;

		protected StaticFileOptions staticFileOptions;

		protected bool useCompression;

		protected bool useMVC;
		#endregion

		#region Constructors
		public FathymViewBuilderPipeline(IApplicationBuilder app)
		{
			this.app = app;
		}
		#endregion

		#region API Methods
		public virtual void Build(IConfigurationRoot config)
		{
			if (staticFileOptions != null)
				buildStaticFiles(config);

			if (useMVC)
				buildMVC(config);

			if (useCompression)
				buildCompression(config);
		}

		public virtual IViewBuilderPipeline Compression()
		{
			useCompression = true;

			return this;
		}

		public virtual IViewBuilderPipeline MVC(Action<IRouteBuilder> configureRoutes = null, bool useDefaultRoutes = false)
		{
			useMVC = true;

			mvcConfigureRoutes = configureRoutes;

			mvcUseDefaultRoutes = useDefaultRoutes;

			return this;
		}

		public virtual IViewBuilderPipeline StaticFiles(StaticFileOptions options)
		{
			staticFileOptions = options;

			return this;
		}


		public virtual IViewBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action)
		{
			appActions.Add(action(app));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void buildActions(IConfigurationRoot config, IHostingEnvironment env)
		{
			appActions.ForEach(action => action());

			appActions.Clear();
		}

		protected virtual void buildCompression(IConfigurationRoot config)
		{
			app.UseResponseCompression();
		}

		protected virtual void buildMVC(IConfigurationRoot config)
		{
			if (mvcConfigureRoutes != null)
				app.UseMvc(mvcConfigureRoutes);
			else if (mvcUseDefaultRoutes)
				app.UseMvcWithDefaultRoute();
			else
				app.UseMvc();
		}

		protected virtual void buildStaticFiles(IConfigurationRoot config)
		{
			app.UseStaticFiles(staticFileOptions);
		}
		#endregion
	}

	public interface IViewBuilderPipeline
	{
		IViewBuilderPipeline Compression();

		IViewBuilderPipeline MVC(Action<IRouteBuilder> configureRoutes = null, bool useDefaultRoutes = false);

		IViewBuilderPipeline StaticFiles(StaticFileOptions options);

		IViewBuilderPipeline WithApp(Func<IApplicationBuilder, Action> action);

		void Build(IConfigurationRoot config);
	}
	#endregion
}
