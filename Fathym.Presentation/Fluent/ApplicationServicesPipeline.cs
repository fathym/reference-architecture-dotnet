using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
using Microsoft.AspNetCore.Identity;
using System.Threading;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Fathym.Presentation.Fluent
{
	public class ApplicationServicesPipeline : IServicesPipelineStartup
	{
		#region Fields
		protected readonly IServiceCollection services;

		protected IConfiguration config;

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
		public virtual void Configure()
		{
			services.AddSingleton<IServicesPipelineStartup>(this);
		}

		public virtual void ConfigureAll(string sessionCookieName)
		{
			this
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

		public virtual IServicesPipelineStartup SetupIdentity<TUser, TRole, TUserStore, TRoleStore>(Action<IdentityOptions> configureOptions = null, 
			Action<CookieAuthenticationOptions> configureCookie = null)
			where TUser : class
			where TRole : class
			where TRoleStore : class
			where TUserStore : class
		{
			services.AddIdentity<TUser, TRole>()
				.AddRoleStore<TRoleStore>()
				.AddUserStore<TUserStore>()
				.AddDefaultTokenProviders();

			if (configureOptions != null)
				services.Configure(configureOptions);
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

			if (configureOptions != null)
				services.ConfigureApplicationCookie(configureCookie);

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
		#endregion
	}

	public interface IServicesPipelineStartup
	{
		void Configure();

		void ConfigureAll(string sessionCookieName);

		IServicesPipelineStartup SetIsDevelopmentCheck(Func<bool> check);

		IServicesPipelineStartup SetupCaching();

		IServicesPipelineStartup SetupCompression();

		IServicesPipelineStartup SetupConfig();

		IServicesPipelineStartup SetupDataProtection(string connectionConfig, string containerConfig);

		IServicesPipelineStartup SetupIdentity<TUser, TRole, TUserStore, TRoleStore>(Action<IdentityOptions> configureOptions = null,
			Action<CookieAuthenticationOptions> configureCookie = null)
			where TUser : class
			where TRole : class
			where TRoleStore : class
			where TUserStore : class;

		IServicesPipelineStartup SetupMVC();

		IServicesPipelineStartup SetupPrerender(string prerenderConfig);

		IServicesPipelineStartup SetupProxy(string proxyConfig);

		IServicesPipelineStartup SetupSessions(string sessionCookieName, int sessionIdleTimeout);

		IServicesPipelineStartup With(Action<IServiceCollection, IConfiguration> action);
	}

	public class UserStore : IUserStore<IdentityUser>
	{
		public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class RoleStore : IRoleStore<IdentityRole>
	{
		public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
