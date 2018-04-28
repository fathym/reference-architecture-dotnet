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
   public class GenericQueryParamProcessor : BaseQueryParamProcessor
	{
		#region Properties
		protected readonly Func<HttpContext, string> queryValueLoaderDelegate;

		protected readonly Func<HttpContext, bool> shouldRemoveDelegate;
		#endregion

		#region Constructors
		public GenericQueryParamProcessor(List<string> queryParams, 
			Func<HttpContext, string> queryValueLoaderDelegate, Func<HttpContext, bool> shouldRemoveDelegate) 
			: base(queryParams)
		{
			this.queryValueLoaderDelegate = queryValueLoaderDelegate;

			this.shouldRemoveDelegate = shouldRemoveDelegate;
		}
		#endregion

		#region API Methods
		#endregion

		#region Helpers
		protected override string[] queryValueLoader(HttpContext context)
		{
			return new[] { queryValueLoaderDelegate(context) };
		}

		protected override bool shouldRemove(HttpContext context)
		{
			return shouldRemoveDelegate(context);
		}
		#endregion
	}
}
