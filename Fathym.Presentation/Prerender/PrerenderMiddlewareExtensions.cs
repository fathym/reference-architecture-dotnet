using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Prerender
{
	public static class PrerenderMiddlewareExtensions
	{
		public static IApplicationBuilder UsePrerender(this IApplicationBuilder app, PrerenderConfiguration configuration = null)
		{
			return app.UseMiddleware<PrerenderMiddleware>(configuration ?? app.ApplicationServices.GetService<IOptions<PrerenderConfiguration>>().Value);
		}
	}
}
