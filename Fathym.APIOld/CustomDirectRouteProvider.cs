﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Fathym.API
{
	public class CustomDirectRouteProvider : DefaultDirectRouteProvider
	{
		protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
		{
			return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: true);
		}
	}
}
