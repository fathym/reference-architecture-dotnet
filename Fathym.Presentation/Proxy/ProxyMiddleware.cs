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
		#endregion

		public ProxyMiddleware(RequestDelegate next, IFabricAdapter fabricAdapter, IOptions<ProxyConfiguration> config)
			: base(next)
		{
			proxyService = new FabricProxyService(fabricAdapter, config);
		}

		public virtual async Task Invoke(HttpContext context)
		{
			var proxied = await proxyService.Proxy(context);

			if (!proxied)
				await next(context);
		}
	}
}
