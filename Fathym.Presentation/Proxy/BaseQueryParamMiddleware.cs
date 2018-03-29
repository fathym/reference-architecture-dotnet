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
	public abstract class BaseQueryParamMiddleware : BaseMiddleware
	{
		#region Properties
		public virtual List<string> QueryParameters { get; protected set; }
		#endregion

		#region Constructors
		public BaseQueryParamMiddleware(RequestDelegate next, List<string> queryParams)
			: base(next)
		{
			QueryParameters = queryParams;
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var request = context.Request;

			var query = QueryHelpers.ParseQuery(request.QueryString.Value).ToDictionary(v => v.Key, v => v.Value.ToString());

			if (!shouldRemove(context))
			{
				var queryValues = queryValueLoader(context);

				if (queryValues.Length != QueryParameters.Count)
					throw new ArgumentException("The number of query values must match the number of query parameters passed in the constructor.");

				for (var i = 0; i < queryValues.Length; i++)
					query[QueryParameters[i]] = queryValues[i];
			}
			else
			{
				QueryParameters.ForEach(qp => query.Remove(qp));
			}

			var currentUri = request.GetFullUrl();

			if (!request.Query.IsNullOrEmpty())
				currentUri = currentUri.Replace(WebUtility.UrlDecode(request.QueryString.Value), String.Empty);

			var newUri = new Uri(QueryHelpers.AddQueryString(currentUri, query));

			context.Request.QueryString = new QueryString(newUri.Query);

			await next(context);
		}
		#endregion

		#region Helpers
		protected abstract string[] queryValueLoader(HttpContext context);

		protected abstract bool shouldRemove(HttpContext context);
		#endregion
	}
}
