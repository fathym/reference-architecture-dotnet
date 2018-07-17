using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Rewrite
{
	public class RedirectWWWRule : IRule
	{
		#region Properties
		public virtual bool IncludeLocalhost { get; set; }
		#endregion

		#region API Methods
		public virtual void ApplyRule(RewriteContext context)
		{
			var request = context.HttpContext.Request;

			var host = request.Host;

			if (host.Host.StartsWith("www", StringComparison.OrdinalIgnoreCase) || host.Host.Count(h => h == '.') > 1 || 
				(!IncludeLocalhost && host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)))
				context.Result = RuleResult.ContinueRules;
			else
			{
				var newPath = request.Scheme + "://www." + host.Value + request.PathBase + request.Path + request.QueryString;

				var response = context.HttpContext.Response;

				response.StatusCode = (int)HttpStatusCode.MovedPermanently;

				response.Headers["Location"] = newPath;

				context.Result = RuleResult.EndResponse;
			}
		}
		#endregion
	}
}
