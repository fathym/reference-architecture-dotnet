using Fathym.Presentation.Proxy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fathym.Presentation.MVC.Fluent
{
	public class FathymApplicationServicesPipeline : IApplicationServicesPipeline
	{
		#region Fields
		protected readonly IServiceCollection services;
		#endregion

		#region Constructors
		public FathymApplicationServicesPipeline(IServiceCollection services)
		{
			this.services = services;
		}
		#endregion

		public ICoreServicesPipeline Core()
		{
			return new FathymCoreServicesPipeline(services);
		}

		public IViewServicesPipeline View()
		{
			return new FathymViewServicesPipeline(services);
		}
	}

	public interface IApplicationServicesPipeline
	{
		ICoreServicesPipeline Core();

		IViewServicesPipeline View();
	}

	#region Core
	public class FathymCoreServicesPipeline : ICoreServicesPipeline
	{
		#region Fields
		protected Action<ResponseCompressionOptions> compressionConfigure;

		protected string dataProtectionConnectionConfig;

		protected string dataProtectionContainerConfig;

		protected readonly IServiceCollection services;

		protected Action<SessionOptions> sessionConfigure;

		protected bool useCaching;

		protected bool useCompression;

		protected bool useConfig;
		#endregion

		#region Constructors
		public FathymCoreServicesPipeline(IServiceCollection services)
		{
			this.services = services;
		}
		#endregion

		#region API Methods
		public virtual ICoreServicesPipeline Caching()
		{
			useCaching = true;

			return this;
		}

		public virtual ICoreServicesPipeline Compression(Action<ResponseCompressionOptions> configure = null)
		{
			useCompression = true;

			compressionConfigure = configure;

			return this;
		}

		public virtual ICoreServicesPipeline Config()
		{
			useConfig = true;

			return this;
		}

		public virtual ICoreServicesPipeline DataProtection(string connectionConfig, string containerConfig)
		{
			dataProtectionConnectionConfig = connectionConfig;

			dataProtectionContainerConfig = containerConfig;

			return this;
		}

		public virtual ICoreServicesPipeline Sessions(Action<SessionOptions> configure)
		{
			sessionConfigure = configure;

			return this;
		}

		public virtual void Set(IConfigurationRoot config)
		{
			if (useCaching)
				setCaching(config);

			if (useCompression)
				setCompression(config);

			if (useConfig)
				setConfig(config);

			if (!dataProtectionConnectionConfig.IsNullOrEmpty() && !dataProtectionContainerConfig.IsNullOrEmpty())
				setDataProtection(config);

			if (sessionConfigure != null)
				setSession(config);
		}
		#endregion

		#region Helpers
		protected virtual void setCaching(IConfigurationRoot config)
		{
			services.AddMemoryCache();
		}

		protected virtual void setCompression(IConfigurationRoot config)
		{
			if (compressionConfigure != null)
				services.AddResponseCompression(compressionConfigure);
			else
				services.AddResponseCompression();
		}

		protected virtual void setConfig(IConfigurationRoot config)
		{
			services.AddSingleton(config);

			services.AddOptions();
		}

		protected virtual void setDataProtection(IConfigurationRoot config)
		{
			var connStr = config.GetSection(dataProtectionConnectionConfig).Value;

			var cont = config.GetSection(dataProtectionContainerConfig).Value;

			services.AddDataProtection().PersistKeysToAzureStorage(connStr, cont);
			//	TODO:  Anyway to support this by pulling the connnection and container in real time based on enterprise/application context... instead of hard coded to node deploy
		}

		protected virtual void setSession(IConfigurationRoot config)
		{
			services.AddSession(sessionConfigure);
		}
		#endregion
	}

	public interface ICoreServicesPipeline
	{
		ICoreServicesPipeline Caching();

		ICoreServicesPipeline Config();

		ICoreServicesPipeline Compression(Action<ResponseCompressionOptions> configureOptions = null);

		ICoreServicesPipeline DataProtection(string connectionConfig, string containerConfig);

		ICoreServicesPipeline Sessions(Action<SessionOptions> sessionConfigure);

		void Set(IConfigurationRoot config);
	}
	#endregion

	#region View
	public class FathymViewServicesPipeline : IViewServicesPipeline
	{
		#region Fields
		protected Action<CookieAuthenticationOptions> identityConfigureCookie;

		protected Action<IdentityOptions> identityConfigureOptions;

		protected Action identityStoreSetup;

		protected List<Assembly> mvcPartAssemblies;

		protected bool mvcDefaultContractResolver;

		protected Type proxyServiceType;

		protected readonly IServiceCollection services;

		protected bool useMvc;
		#endregion

		#region Constructors
		public FathymViewServicesPipeline(IServiceCollection services)
		{
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

			return this;
		}

		public virtual IViewServicesPipeline MVC(List<Assembly> assemblies = null, bool defaultContractResolver = true)
		{
			useMvc = true;

			mvcPartAssemblies = assemblies;

			mvcDefaultContractResolver = defaultContractResolver;

			return this;
		}

		public virtual IViewServicesPipeline Proxy<TProxyService>()
			where TProxyService : class, IProxyService
		{
			proxyServiceType = typeof(TProxyService);

			return this;
		}

		public virtual void Set(IConfigurationRoot config)
		{
			if (identityStoreSetup != null)
				setIdentity(config);

			if (useMvc)
				setMVC(config);

			if (proxyServiceType != null)
				setProxy(config);
		}
		#endregion

		#region Helpers
		protected virtual void setIdentity(IConfigurationRoot config)
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

		protected virtual void setMVC(IConfigurationRoot config)
		{
			var mvcBuilder = services.AddMvc();

			if (mvcDefaultContractResolver)
				mvcBuilder.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

			mvcPartAssemblies.ForEach(assembly => mvcBuilder.AddApplicationPart(assembly));
		}

		protected virtual void setProxy(IConfigurationRoot config)
		{
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

		IViewServicesPipeline Proxy<TProxyService>() where TProxyService : class, IProxyService;

		void Set(IConfigurationRoot config);
	}
	#endregion
}
