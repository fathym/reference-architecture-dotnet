using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
	public class ProxyResult
	{
		#region Properties
		public virtual HttpStatusCode StatusCode { get; protected set; }

		public virtual string Body { get; protected set; }

		public virtual HttpResponseHeaders Headers { get; protected set; }
		#endregion

		#region Constructors
		public ProxyResult(HttpStatusCode code, string body, HttpResponseHeaders headers)
		{
			StatusCode = code;

			Body = body;

			Headers = headers;
		} 
		#endregion

	}
}
