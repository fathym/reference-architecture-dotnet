using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Prerender
{
	public class PrerenderMiddleware : BaseMiddleware
	{
		#region Fields
		protected readonly PrerenderConfiguration configuration;

		protected PrerenderRequestHelper helper;
		#endregion

		#region Constructors
		public PrerenderMiddleware(RequestDelegate next, PrerenderConfiguration configuration)
			: base(next)
		{
			this.configuration = configuration;

			helper = new PrerenderRequestHelper(configuration);
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var handledPrerender = await helper.HandlePrerender(context);

			if (!handledPrerender)
				await next(context);
		} 
		#endregion
	}
}
