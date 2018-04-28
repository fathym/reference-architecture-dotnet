using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
	public class ProxyMiddleware : BaseMiddleware
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;

		protected readonly IProxyService proxyService;

		protected readonly IDictionary<string, IQueryParamProcessor> queryParamProcessors;
		#endregion

		public ProxyMiddleware(RequestDelegate next, IProxyService proxyService, IDictionary<string, IQueryParamProcessor> queryParamProcessors)
			: base(next)
		{
			this.proxyService = proxyService;

			this.queryParamProcessors = queryParamProcessors;
		}

		public virtual async Task Invoke(HttpContext context)
		{
			var proxied = await proxyService.Proxy(context, queryParamProcessors);

			if (!proxied)
				await next(context);
		}
	}
}
