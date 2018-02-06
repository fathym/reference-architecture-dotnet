using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation
{
	public static class HttpRequestExtensions
	{
		public static string GetFullUrl(this HttpRequest request)
		{
			if (request.Path == "/")
				return $"{request.Scheme}://{request.Host.Value}";

			return $"{request.Scheme}://{request.Host.Value}{request.Path}";
		}

		public static string GetUserAgent(this HttpRequest request)
		{
			return request.Headers.ContainsKey("User-Agent") ? (string)request.Headers["User-Agent"] : string.Empty;
		}
	}
}
