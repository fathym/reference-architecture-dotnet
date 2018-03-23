using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Presentation.Proxy
{
	[Serializable]
	public class ProxyContext
	{
		public const string Lookup = "Fathym:Proxy";

		public virtual int? ConfigCacheDurationSeconds { get; set; }

		public virtual int? WebSocketBufferSize { get; set; }

		public virtual string[] NotForwardedWebSocketHeaders { get; set; }

		public virtual ProxyConnection Proxy { get; set; }

		public virtual int? StreamCopyBufferSize { get; set; }

		public virtual TimeSpan? WebSocketKeepAliveInterval { get; set; }
	}

	[Serializable]
	public class ProxyConnection
	{
		public virtual string Application { get; set; }

		public virtual string Service { get; set; }
	}

	[Serializable]
	public class ProxyOptions
	{
		public virtual int ConfigCacheDurationSeconds { get; set; }

		public virtual int WebSocketBufferSize { get; set; }

		public virtual string[] NotForwardedWebSocketHeaders { get; set; }

		public virtual ProxyConnection Proxy { get; set; }

		public virtual int StreamCopyBufferSize { get; set; }

		public virtual TimeSpan? WebSocketKeepAliveInterval { get; set; }
	}
}
