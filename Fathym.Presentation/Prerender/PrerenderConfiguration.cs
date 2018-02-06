using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Prerender
{
	public class PrerenderConfiguration
	{
		public virtual string AdditionalExtensionPattern { get; set; }

		public virtual string BlackListPattern { get; set; }

		public virtual string CrawlerUserAgentPattern { get; internal set; }

		public virtual List<string> ExtensionsToIgnore { get; set; }

		public virtual string PrerenderServiceUrl { get; set; }

		public virtual PrerenderProxy Proxy { get; set; }

		public virtual bool StripApplicationNameFromRequestUrl { get; set; } = false;

		public virtual string Token { get; set; }

		public virtual string WhiteListPattern { get; set; }
	}

	public class PrerenderProxy
	{
		public virtual int Port { get; set; } = 80;

		public virtual string URL { get; set; }
	}
}
