using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
	public class FabricProxyService : GenericProxyService
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;
		#endregion

		#region Constructors
		public FabricProxyService(IFabricAdapter fabricAdapter, IOptions<ProxyConfiguration> config)
			: base(config)
		{
			this.fabricAdapter = fabricAdapter;
		}
		#endregion

		#region Helpers
		protected override async Task<Uri> resolveDestinationUri(HttpContext context, ProxyOptions proxyOptions)
		{
			var partResolve = ServicePartitionResolver.GetDefault();

			var serviceUri = fabricAdapter.LoadServiceUri(proxyOptions.Proxy.Application, proxyOptions.Proxy.Service);

			var resolved = await partResolve.ResolveAsync(serviceUri, new ServicePartitionKey(), new System.Threading.CancellationToken());

			var endpoint = resolved.Endpoints.FirstOrDefault();

			var resolvedUri = new Uri(endpoint.Address);

			return new Uri(UriHelper.BuildAbsolute(resolvedUri.Scheme, new HostString(resolvedUri.Host, resolvedUri.Port), path: context.Request.Path,
				query: context.Request.QueryString));
		}

		protected override IProxyRequestHandler resolveProxyRequestHandler(HttpContext context, ProxyOptions proxyOptions)
		{
			if (context.WebSockets.IsWebSocketRequest)
				return new WebSocketProxyRequestHandler(proxyOptions, fabricAdapter);
			else
				return new HttpClientProxyRequestHandler(proxyOptions, fabricAdapter);	
		}
		#endregion
	}
}
