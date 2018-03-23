using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fathym.Presentation.Proxy;
using Fathym.Design;
using Fathym.Fabric.Communications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Fathym.Presentation.Proxy
{
	public abstract class GenericProxyService : IProxyService
	{
		#region Fields
		protected readonly ProxyConfiguration config;
		#endregion

		#region Constructors
		public GenericProxyService(IOptions<ProxyConfiguration> config)
		{
			this.config = config.Value;
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> Proxy(HttpContext context)
		{
			var proxyOptions = resolveProxyOptions(context);

			if (proxyOptions != null)
			{
				if (proxyOptions.ProxyPath == null)
					await resolveProxyPath(context, proxyOptions);

				var reqHndlr = resolveProxyRequestHandler(context, proxyOptions);

				if (reqHndlr != null)
				{
					return await reqHndlr.Proxy(context);
				}
				else
					return Status.GeneralError.Clone("Proxy Request Handler not located.");
			}
			else
				return Status.GeneralError.Clone("Proxy options not located.");
		}

		protected abstract Task resolveProxyPath(HttpContext context, ProxyOptions proxyOptions);

		protected virtual ProxyOptions resolveProxyContextToOptions(ProxyContext proxyContext)
		{
			if (proxyContext == null)
				return null;

			var proxyOptions = new ProxyOptions()
			{
				Proxy = proxyContext.Proxy
			};

			if (proxyContext.Authorization != null)
				proxyOptions.Authorization = proxyContext.Authorization;

			proxyOptions.ConfigCacheDurationSeconds = !proxyContext.ConfigCacheDurationSeconds.HasValue ?
				proxyContext.ConfigCacheDurationSeconds.Value : config.ConfigCacheDurationSeconds;

			proxyOptions.NotForwardedWebSocketHeaders = !proxyContext.NotForwardedWebSocketHeaders.IsNullOrEmpty() ?
				proxyContext.NotForwardedWebSocketHeaders : config.NotForwardedWebSocketHeaders;

			if (proxyContext.ProxyPath != null)
				proxyOptions.ProxyPath = proxyContext.ProxyPath;

			proxyOptions.StreamCopyBufferSize = !proxyContext.StreamCopyBufferSize.HasValue ?
				proxyContext.StreamCopyBufferSize.Value : config.StreamCopyBufferSize;

			proxyOptions.WebSocketBufferSize = !proxyContext.WebSocketBufferSize.HasValue ?
				proxyContext.WebSocketBufferSize.Value : config.DefaultWebSocketBufferSize;

			proxyOptions.WebSocketKeepAliveInterval = !proxyContext.WebSocketKeepAliveInterval.HasValue ?
				proxyContext.WebSocketKeepAliveInterval.Value : config.WebSocketKeepAliveInterval;

			return proxyOptions;
		}

		protected virtual ProxyOptions resolveProxyOptions(HttpContext context)
		{
			var proxyContext = context.ResolveContext<ProxyContext>(ProxyContext.Lookup);

			var proxyOptions = resolveProxyContextToOptions(proxyContext);

			return proxyOptions;
		}

		protected abstract IProxyRequestHandler resolveProxyRequestHandler(HttpContext context, ProxyOptions proxyOptions);
		#endregion

		#region Helpers

		#endregion
	}
}
