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
using Microsoft.AspNetCore.WebUtilities;

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
		public virtual async Task<Status> Proxy(HttpContext context, IDictionary<string, IQueryParamProcessor> queryParamProcessors)
		{
			var proxyContext = context.ResolveContext<ProxyContext>(ProxyContext.Lookup);

			if (!isValidProxyContext(proxyContext))
				return null;

			await finalizeProxyContext(context, queryParamProcessors);

			var proxyOptions = resolveProxyOptions(context);

			if (proxyOptions != null)
			{
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
		#endregion

		#region Helpers
		protected virtual async Task finalizeProxyContext(HttpContext context, IDictionary<string, IQueryParamProcessor> queryParamProcessors)
		{
			await context.HandleContext<ProxyContext>(ProxyContext.Lookup,
				async (proxyContext) =>
				{
					if (proxyContext.Proxy.Path.IsNullOrEmpty())
					{
						string query;
						proxyContext.Proxy.Path = await resolveProxyPath(context, out query);

						proxyContext.Proxy.Query = query;
					}
				});

			await processQueryParams(context, queryParamProcessors);
		}

		protected virtual bool isValidProxyContext(ProxyContext proxyContext)
		{
			return proxyContext != null && proxyContext.Proxy != null && proxyContext.Proxy.Connection != null &&
				!proxyContext.Proxy.Connection.Application.IsNullOrEmpty() && !proxyContext.Proxy.Connection.Service.IsNullOrEmpty();
		}

		protected virtual async Task processQueryParams(HttpContext context, IDictionary<string, IQueryParamProcessor> queryParamProcessors)
		{
			var proxyContext = context.ResolveContext<ProxyContext>(ProxyContext.Lookup);

			if (queryParamProcessors.IsNullOrEmpty() || proxyContext == null || proxyContext.Proxy == null ||
				proxyContext.Proxy.QueryParamProcessors.IsNullOrEmpty())
				return;

			foreach (var qpp in queryParamProcessors)
			{
				if (proxyContext.Proxy.QueryParamProcessors.Contains(qpp.Key))
					await qpp.Value.Process(context);
			}
		}

		protected abstract Task<string> resolveProxyPath(HttpContext context, out string query);

		protected virtual ProxyOptions resolveProxyContextToOptions(ProxyContext proxyContext)
		{
			var proxyOptions = new ProxyOptions()
			{
				Proxy = proxyContext.Proxy
			};

			proxyOptions.ConfigCacheDurationSeconds = proxyContext.ConfigCacheDurationSeconds.HasValue ?
				proxyContext.ConfigCacheDurationSeconds.Value : config.ConfigCacheDurationSeconds;

			proxyOptions.NotForwardedWebSocketHeaders = !proxyContext.NotForwardedWebSocketHeaders.IsNullOrEmpty() ?
				proxyContext.NotForwardedWebSocketHeaders : config.NotForwardedWebSocketHeaders;

			proxyOptions.StreamCopyBufferSize = proxyContext.StreamCopyBufferSize.HasValue ?
				proxyContext.StreamCopyBufferSize.Value : config.StreamCopyBufferSize;

			proxyOptions.WebSocketBufferSize = proxyContext.WebSocketBufferSize.HasValue ?
				proxyContext.WebSocketBufferSize.Value : config.DefaultWebSocketBufferSize;

			proxyOptions.WebSocketKeepAliveInterval = proxyContext.WebSocketKeepAliveInterval.HasValue ?
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
	}
}
