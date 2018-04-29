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
	public abstract class BaseQueryParamProcessor : IQueryParamProcessor
	{
		#region Properties
		public virtual List<string> QueryParameters { get; protected set; }
		#endregion

		#region Constructors
		public BaseQueryParamProcessor(List<string> queryParams)
		{
			QueryParameters = queryParams;
		}
		#endregion

		#region API Methods
		public virtual async Task Process(HttpContext context)
		{
			await context.HandleContext<ProxyContext>(ProxyContext.Lookup, 
				async (proxyContext) =>
				{
					if (proxyContext == null || proxyContext.Proxy == null)
						throw new ArgumentException("The proxy context must be set prior to query param processing");

					if (proxyContext.Proxy.Query.IsNullOrEmpty())
						proxyContext.Proxy.Query = "";

					var query = QueryHelpers.ParseQuery(proxyContext.Proxy.Query).ToDictionary(v => v.Key, v => v.Value.ToString());

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

					var uriBldr = new UriBuilder();
					uriBldr.Host = proxyContext.Proxy.Host ?? "xxx";
					uriBldr.Path = proxyContext.Proxy.Path;

					var currentUri = uriBldr.ToString();

					var newUri = new Uri(QueryHelpers.AddQueryString(currentUri, query));

					proxyContext.Proxy.Query = newUri.Query;
				});
		}
		#endregion

		#region Helpers
		protected abstract string[] queryValueLoader(HttpContext context);

		protected abstract bool shouldRemove(HttpContext context);
		#endregion
	}
}
