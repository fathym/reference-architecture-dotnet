using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Compression
{
	public class GZipMiddleware
	{
		#region Fields
		private readonly RequestDelegate next;
		#endregion

		#region Constructors
		public GZipMiddleware(RequestDelegate next)
		{
			this.next = next;
		}
		#endregion

		#region API Methods
		public async Task Invoke(HttpContext httpContext)
		{
			string acceptEncoding = httpContext.Request.Headers["Accept-Encoding"];

			if (!String.IsNullOrEmpty(acceptEncoding))
			{
				if (acceptEncoding.IndexOf("gzip", StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					using (var memoryStream = new MemoryStream())
					{
						var stream = httpContext.Response.Body;

						httpContext.Response.Body = memoryStream;

						if (next != null)
							await next(httpContext);

						using (var compressedStream = new GZipStream(stream, CompressionLevel.Optimal))
						{
							httpContext.Response.Headers.Add("Content-Encoding", new string[] { "gzip" });

							memoryStream.Seek(0, SeekOrigin.Begin);

							await memoryStream.CopyToAsync(compressedStream);
						}
					}
				}
				else
					await next(httpContext);
			}
			else
				await next(httpContext);

		}
		#endregion
	}
}
