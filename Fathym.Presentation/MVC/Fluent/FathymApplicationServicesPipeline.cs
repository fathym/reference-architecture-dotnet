using Fathym.Fluent;
using Fathym.Presentation.Proxy;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fathym.Presentation.MVC.Fluent
{
	public class FathymApplicationServicesPipeline : IApplicationServicesPipeline
	{
		#region Fields
		protected IConfigurationRoot config;

		protected readonly IServiceCollection services;
		#endregion

		#region Constructors
		public FathymApplicationServicesPipeline(IServiceCollection services, IConfigurationRoot config)
		{
			this.config = config;

			this.services = services;
		}
		#endregion

		public ICoreServicesPipeline Core()
		{
			return new FathymCoreServicesPipeline(services, config);
		}

		public IViewServicesPipeline View()
		{
			return new FathymViewServicesPipeline(services, config);
		}
	}

	public interface IApplicationServicesPipeline
	{
		ICoreServicesPipeline Core();

		IViewServicesPipeline View();
	}

	#region Core
	public class FathymCoreServicesPipeline : BaseOrderedPipeline, ICoreServicesPipeline
	{
		#region Fields
		protected Action<ApplicationInsightsServiceOptions> appInsightsConfigure;

		protected Action<ResponseCompressionOptions> compressionConfigure;

		protected IConfigurationRoot config;

		protected string dataProtectionBlobConfig;

		protected string dataProtectionConnectionConfig;

		protected string dataProtectionContainerConfig;

		protected readonly IServiceCollection services;

		protected Action<SessionOptions> sessionConfigure;
		#endregion

		#region Constructors
		public FathymCoreServicesPipeline(IServiceCollection services, IConfigurationRoot config)
		{
			this.config = config;

			this.services = services;
		}
		#endregion

		#region API Methods
		public virtual ICoreServicesPipeline AppInsights(Action<ApplicationInsightsServiceOptions> configure = null)
		{
			appInsightsConfigure = configure;

			addAction(setAppInsights);

			return this;
		}

		public virtual ICoreServicesPipeline Caching()
		{
			addAction(setCaching);

			return this;
		}

		public virtual ICoreServicesPipeline Compression(Action<ResponseCompressionOptions> configure = null)
		{
			compressionConfigure = configure;

			addAction(setCompression);

			return this;
		}

		public virtual ICoreServicesPipeline Config()
		{
			addAction(setConfig);

			return this;
		}

		public virtual ICoreServicesPipeline DataProtection(string connectionConfig, string containerConfig, string blobConfig)
		{
			dataProtectionBlobConfig = blobConfig;

			dataProtectionConnectionConfig = connectionConfig;

			dataProtectionContainerConfig = containerConfig;

			if (!dataProtectionConnectionConfig.IsNullOrEmpty() && !dataProtectionContainerConfig.IsNullOrEmpty() &&
				!dataProtectionBlobConfig.IsNullOrEmpty())
				addAction(setDataProtection);

			return this;
		}

		public virtual ICoreServicesPipeline Sessions(Action<SessionOptions> configure)
		{
			sessionConfigure = configure;

			addAction(setSession);

			return this;
		}

		public virtual ICoreServicesPipeline WithServices(Func<IServiceCollection, Action> action)
		{
			addAction(action(services));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void setAppInsights()
		{
			if (appInsightsConfigure == null)
				services.AddApplicationInsightsTelemetry();
			else
				services.AddApplicationInsightsTelemetry(appInsightsConfigure);
		}

		protected virtual void setCaching()
		{
			services.AddMemoryCache();
		}

		protected virtual void setCompression()
		{
			if (compressionConfigure != null)
				services.AddResponseCompression(compressionConfigure);
			else
				services.AddResponseCompression();
		}

		protected virtual void setConfig()
		{
			services.AddSingleton(config);

			services.AddOptions();
		}

		protected virtual void setDataProtection()
		{
			var connStr = config.GetSection(dataProtectionConnectionConfig).Value;

			var contName = config.GetSection(dataProtectionContainerConfig).Value;

			var blobName = config.GetSection(dataProtectionBlobConfig).Value;

			var account = CloudStorageAccount.Parse(connStr);

			var blobClient = account.CreateCloudBlobClient();

			var container = blobClient.GetContainerReference(contName.ToLower());

			container.CreateIfNotExistsAsync().Result.ToString();

			services.AddDataProtection().PersistKeysToAzureBlobStorage(container, blobName);
			//	TODO:  Anyway to support this by pulling the connnection and container in real time based on enterprise/application context... instead of hard coded to node deploy
		}

		protected virtual void setSession()
		{
			if (sessionConfigure == null)
				services.AddSession();
			else
				services.AddSession(sessionConfigure);
		}
		#endregion
	}

	public interface ICoreServicesPipeline
	{
		ICoreServicesPipeline AppInsights(Action<ApplicationInsightsServiceOptions> configure = null);

		ICoreServicesPipeline Caching();

		ICoreServicesPipeline Config();

		ICoreServicesPipeline Compression(Action<ResponseCompressionOptions> configureOptions = null);

		ICoreServicesPipeline DataProtection(string connectionConfig, string containerConfig, string blobConfig);

		ICoreServicesPipeline Sessions(Action<SessionOptions> sessionConfigure);

		ICoreServicesPipeline WithServices(Func<IServiceCollection, Action> action);

		void Run();
	}
	#endregion

	#region View
	public class FathymViewServicesPipeline : BaseOrderedPipeline, IViewServicesPipeline
	{
		#region Fields
		protected IConfigurationRoot config;

		protected Action<CookieAuthenticationOptions> identityConfigureCookie;

		protected Action<IdentityOptions> identityConfigureOptions;

		protected Action identityStoreSetup;

		protected List<Assembly> mvcPartAssemblies;

		protected bool mvcDefaultContractResolver;

		protected Type proxyServiceType;

		protected Func<HttpContext, ProxyOptions, IProxyRequestHandler> proxyResolveProxyRequestHandler;

		protected readonly IServiceCollection services;
		#endregion

		#region Constructors
		public FathymViewServicesPipeline(IServiceCollection services, IConfigurationRoot config)
		{
			this.config = config;

			this.services = services;
		}
		#endregion

		#region API Methods
		public virtual IViewServicesPipeline Identity<TUser, TRole, TUserStore, TRoleStore>(Action<IdentityOptions> configureOptions = null,
			Action<CookieAuthenticationOptions> configureCookie = null)
			where TUser : class
			where TRole : class
			where TRoleStore : class
			where TUserStore : class
		{
			identityStoreSetup = () =>
			{
				services.AddIdentity<TUser, TRole>()
					.AddRoleStore<TRoleStore>()
					.AddUserStore<TUserStore>()
					.AddDefaultTokenProviders();
			};

			identityConfigureCookie = configureCookie;

			identityConfigureOptions = configureOptions;

			if (identityStoreSetup != null)
				addAction(setIdentity);

			return this;
		}

		public virtual IViewServicesPipeline MVC(List<Assembly> assemblies = null, bool defaultContractResolver = true)
		{
			mvcPartAssemblies = assemblies;

			mvcDefaultContractResolver = defaultContractResolver;

			addAction(setMVC);

			return this;
		}

		public virtual IViewServicesPipeline Proxy<TProxyService>(
			Func<HttpContext, ProxyOptions, IProxyRequestHandler> resolveProxyRequestHandler = null)
			where TProxyService : class, IProxyService
		{
			proxyServiceType = typeof(TProxyService);

			proxyResolveProxyRequestHandler = resolveProxyRequestHandler;

			addAction(setProxy);

			return this;
		}

		public virtual IViewServicesPipeline WithServices(Func<IServiceCollection, Action> action)
		{
			addAction(action(services));

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void setIdentity()
		{
			identityStoreSetup();

			if (identityConfigureOptions != null)
				services.Configure(identityConfigureOptions);
			else
				services.Configure<IdentityOptions>(options =>
				{
					// Password settings
					options.Password.RequireDigit = true;
					options.Password.RequiredLength = 8;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = true;
					options.Password.RequireLowercase = false;
					options.Password.RequiredUniqueChars = 6;

					// Lockout settings
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
					options.Lockout.MaxFailedAccessAttempts = 10;
					options.Lockout.AllowedForNewUsers = true;

					// User settings
					options.User.RequireUniqueEmail = true;
				});

			if (identityConfigureCookie != null)
				services.ConfigureApplicationCookie(identityConfigureCookie);
		}

		protected virtual void setMVC()
		{
			var mvcBuilder = services.AddMvc();

			if (mvcDefaultContractResolver)
				mvcBuilder.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

			mvcPartAssemblies.ForEach(assembly => mvcBuilder.AddApplicationPart(assembly));
		}

		protected virtual void setProxy()
		{
			if (proxyResolveProxyRequestHandler != null)
				services.AddSingleton(typeof(Func<HttpContext, ProxyOptions, IProxyRequestHandler>), proxyResolveProxyRequestHandler);

			services.AddTransient(typeof(IProxyService), proxyServiceType);
		}
		#endregion
	}

	public interface IViewServicesPipeline
	{
		IViewServicesPipeline Identity<TUser, TRole, TUserStore, TRoleStore>(Action<IdentityOptions> configureOptions = null, Action<CookieAuthenticationOptions> configureCookie = null)
			where TUser : class
			where TRole : class
			where TRoleStore : class
			where TUserStore : class;

		IViewServicesPipeline MVC(List<Assembly> assemblies = null, bool defaultContractResolver = true);

		IViewServicesPipeline Proxy<TProxyService>(Func<HttpContext, ProxyOptions, IProxyRequestHandler> resolveProxyRequestHandler = null) where TProxyService : class, IProxyService;

		IViewServicesPipeline WithServices(Func<IServiceCollection, Action> action);

		void Run();
	}
	#endregion
}
