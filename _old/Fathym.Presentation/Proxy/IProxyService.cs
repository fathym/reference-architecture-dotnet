using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
    public interface IProxyService
    {
		Task<Status> Proxy(HttpContext context, IDictionary<string, IQueryParamProcessor> queryParamProcessors);
    }
}
