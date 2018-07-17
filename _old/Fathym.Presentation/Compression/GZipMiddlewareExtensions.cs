using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Compression
{
	public static class GZipMiddlewareExtensions
	{
		public static IApplicationBuilder UseCompression(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<GZipMiddleware>();
		}
	}
}
