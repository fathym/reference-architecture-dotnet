using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
	public static class ProxyExtensions
	{
		public static IApplicationBuilder UseClientIPQueryParam(this IApplicationBuilder app, List<string> queryParams)
		{
			return app.UseMiddleware<ClientIPQueryParamMiddleware>(queryParams);
		}

		public static IApplicationBuilder UseUsernameQueryParam(this IApplicationBuilder app, List<string> queryParams)
		{
			return app.UseMiddleware<UsernameQueryParamMiddleware>(queryParams);
		}
	}
}
