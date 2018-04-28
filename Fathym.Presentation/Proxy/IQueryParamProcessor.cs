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
	public interface IQueryParamProcessor
	{
		Task Process(HttpContext context);
	}
}
