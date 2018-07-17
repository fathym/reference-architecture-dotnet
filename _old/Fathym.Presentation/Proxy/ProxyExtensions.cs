using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Presentation.Proxy
{
    public static class ProxyExtensions
	{
		public static Uri ToWebSocketScheme(this Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			var uriBuilder = new UriBuilder(uri);
			if (string.Equals(uriBuilder.Scheme, "https", StringComparison.OrdinalIgnoreCase))
			{
				uriBuilder.Scheme = "wss";
			}
			else if (string.Equals(uriBuilder.Scheme, "http", StringComparison.OrdinalIgnoreCase))
			{
				uriBuilder.Scheme = "ws";
			}

			return uriBuilder.Uri;
		}

		public static IApplicationBuilder UseProxy(this IApplicationBuilder app, IDictionary<string, IQueryParamProcessor> queryParamProcessors)
		{
			return app.UseMiddleware<ProxyMiddleware>(queryParamProcessors);
		}
	}
}
