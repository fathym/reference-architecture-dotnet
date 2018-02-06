using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
   public class UsernameQueryParamMiddleware : BaseQueryParamMiddleware
	{
		#region Properties
		#endregion

		#region Constructors
		public UsernameQueryParamMiddleware(RequestDelegate next, List<string> queryParams) 
			: base(next, queryParams)
		{ }
		#endregion

		#region API Methods
		#endregion

		#region Helpers
		protected override string[] queryValueLoader(HttpContext context)
		{
			return new[] { context.User.Identity.Name };
		}

		protected override bool shouldRemove(HttpContext context)
		{
			return !context.User.Identity.IsAuthenticated;
		}
		#endregion
	}
}
