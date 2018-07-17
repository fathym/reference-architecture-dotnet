using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
    public interface IProxyRequestHandler
	{
		Task<Status> Proxy(HttpContext context);
    }
}
