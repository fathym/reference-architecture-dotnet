using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;

namespace Fathym.Fabric.Runtime.Host
{
	public class KestrelStatelessServiceHost<TStartup> : StatelessService
		where TStartup : class
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;

		protected readonly string serviceEndpointName;
		#endregion

		#region Constructors
		public KestrelStatelessServiceHost(string serviceEndpointName, StatelessServiceContext context)
			: base(context)
		{
			fabricAdapter = new StatelessFabricAdapter(context);

			this.serviceEndpointName = serviceEndpointName;
		}
		#endregion

		#region Runtime
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new ServiceInstanceListener[]
			{
				new ServiceInstanceListener(buildCommunicationListener)
			};
		}
		#endregion

		#region  Helpers
		protected virtual ICommunicationListener buildCommunicationListener(StatelessServiceContext context)
		{
			return new KestrelCommunicationListener(context, serviceEndpointName, (url, listener) =>
			{
				FabricEventSource.Current.ServiceMessage(Context, $"Starting Kestrel on {url}");

				return buildWebHost(url, listener);
			});
		}

		protected virtual IWebHost buildWebHost(string url, AspNetCoreCommunicationListener listener)
		{
			var aiKey = fabricAdapter.GetConfiguration().LoadConfigSetting<string>("EventFlow", "AI.Key");

			return new WebHostBuilder()
				.UseKestrel(
					options =>
					{
						options.Limits.MaxRequestHeaderCount = 32;

						options.Limits.MaxRequestHeadersTotalSize = 2000000;//  Is this really how to handle the HTTP 431 error we were getting?

						options.Limits.MaxRequestBodySize = 2000000;//  Is this really how to handle the HTTP 431 error we were getting?

						options.Limits.MaxRequestBufferSize = options.Limits.MaxRequestHeadersTotalSize + options.Limits.MaxRequestBodySize;
					})
				.ConfigureServices(
					services =>
					{
						services.AddSingleton(fabricAdapter);
					})
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<TStartup>()
				.UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
				.UseApplicationInsights(aiKey)
				.UseUrls(url)
				.Build();
		}
		#endregion
	}
}
