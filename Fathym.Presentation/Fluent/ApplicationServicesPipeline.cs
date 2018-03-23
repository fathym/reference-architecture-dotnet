using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.Extensions.Logging;
using Fathym.Presentation.Prerender;
using Fathym.Presentation.Proxy;

namespace Fathym.Presentation.Fluent
{
	public class ApplicationServicesPipeline : IServicesPipelineStartup
	{
		#region Fields
		protected readonly IServiceCollection services;

		protected IConfiguration config;

		protected IContainer container;

		protected readonly IHostingEnvironment env;

		protected readonly IFabricAdapter fabricAdapter;

		protected Func<bool> isDevelopment;

		protected readonly ILoggerFactory loggerFactory;
		#endregion

		#region Constructors
		public ApplicationServicesPipeline(IServiceCollection services, IConfiguration config, IHostingEnvironment env, IFabricAdapter fabricAdapter, ILoggerFactory loggerFactory)
		{
			this.config = config;

			this.env = env;

			this.fabricAdapter = fabricAdapter;

			this.loggerFactory = loggerFactory;

			isDevelopment = () => env.IsDevelopment();

			this.services = services;
		}
		#endregion

		#region API Methods
		public virtual IServiceProvider Configure()
		{
			return buildServiceProvider(services);
		}

		public virtual IServiceProvider ConfigureAll(string sessionCookieName)
		{
			return this
				.SetupConfig()
				.SetupCaching()
				.SetupCompression()
				.SetupSessions(sessionCookieName, 30)
				.SetupMVC()
				.SetupPrerender("Prerender")
				.SetupDataProtection("Data:Protection:Connection", "Data:Protection:Container")
				.SetupProxy("Proxy")
				.Configure();
		}

		public virtual IServicesPipelineStartup SetIsDevelopmentCheck(Func<bool> check)
		{
			isDevelopment = check;

			return this;
		}

		public virtual IServicesPipelineStartup SetupCaching()
		{
			services.AddMemoryCache();

			return this;
		}

		public virtual IServicesPipelineStartup SetupCompression()
		{
			//services.AddResponseCompression(o =>
			//{
			//	o.EnableForHttps = true;

			//	o.MimeTypes.ToString();
			//});

			return this;
		}

		public virtual IServicesPipelineStartup SetupConfig()
		{
			services.AddSingleton(config);

			services.AddOptions();

			return this;
		}

		public virtual IServicesPipelineStartup SetupDataProtection(string connectionConfig, string containerConfig)
		{
			var connStr = config.GetSection(connectionConfig).Value;

			var cont = config.GetSection(containerConfig).Value;

			services.AddDataProtection().PersistKeysToAzureStorage(connStr, cont);

			return this;
		}

		public virtual IServicesPipelineStartup SetupMVC()
		{
			services.AddMvc();

			return this;
		}

		public virtual IServicesPipelineStartup SetupPrerender(string prerenderConfig)
		{
			//if (!isDevelopment())
			services.Configure<PrerenderConfiguration>(config.GetSection(prerenderConfig));

			return this;
		}

		public virtual IServicesPipelineStartup SetupProxy(string proxyConfig)
		{
			services.Configure<ProxyConfiguration>(config.GetSection(proxyConfig));

			return this;
		}

		public virtual IServicesPipelineStartup SetupSessions(string sessionCookieName, int sessionIdleTimeout)
		{
			services.AddSession(o =>
			{
				o.IdleTimeout = TimeSpan.FromMinutes(sessionIdleTimeout);

				o.CookieName = sessionCookieName;
			});

			return this;
		}

		public virtual IServicesPipelineStartup With(Action<IServiceCollection, IConfiguration> action)
		{
			action(services, config);

			return this;
		}
		#endregion

		#region Helpers
		protected virtual IContainer buildContainer(ContainerBuilder builder)
		{
			return builder.Build();
		}

		protected virtual IServiceProvider buildServiceProvider(IServiceCollection services)
		{
			services.AddSingleton<IServicesPipelineStartup>(this);

			var builder = new ContainerBuilder();

			builder.Populate(services);

			container = buildContainer(builder);

			return container.Resolve<IServiceProvider>();
		}
		#endregion
	}

	public interface IServicesPipelineStartup
	{
		IServiceProvider Configure();

		IServiceProvider ConfigureAll(string sessionCookieName);

		IServicesPipelineStartup SetIsDevelopmentCheck(Func<bool> check);

		IServicesPipelineStartup SetupCaching();

		IServicesPipelineStartup SetupCompression();

		IServicesPipelineStartup SetupConfig();

		IServicesPipelineStartup SetupDataProtection(string connectionConfig, string containerConfig);

		IServicesPipelineStartup SetupMVC();

		IServicesPipelineStartup SetupPrerender(string prerenderConfig);

		IServicesPipelineStartup SetupProxy(string proxyConfig);

		IServicesPipelineStartup SetupSessions(string sessionCookieName, int sessionIdleTimeout);

		IServicesPipelineStartup With(Action<IServiceCollection, IConfiguration> action);
	}
}
