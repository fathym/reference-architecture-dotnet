﻿using Fathym.Fabric.Runtime.Adapters;
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
		protected override async Task<string> resolveProxyPath(HttpContext context, ProxyOptions proxyOptions)
		{
			var uri = new Uri(UriHelper.BuildAbsolute("http", new HostString("xxx", 80), path: context.Request.Path, query: context.Request.QueryString));

			return uri.PathAndQuery;
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
