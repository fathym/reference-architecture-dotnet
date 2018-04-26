using Fathym.Fabric.Runtime.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Proxy
{
	public class ProxyConfiguration
	{
		public virtual int ConfigCacheDurationSeconds { get; set; }

		public virtual int DefaultWebSocketBufferSize { get; set; }

		public virtual string[] NotForwardedWebSocketHeaders { get; set; }

		public virtual int StreamCopyBufferSize { get; set; }

		public virtual TimeSpan? WebSocketKeepAliveInterval { get; set; }

		public ProxyConfiguration()
		{
			DefaultWebSocketBufferSize = 4096;

			NotForwardedWebSocketHeaders = new[] { "Connection", "Host", "Upgrade", "Sec-WebSocket-Accept", "Sec-WebSocket-Protocol", "Sec-WebSocket-Key", "Sec-WebSocket-Version", "Sec-WebSocket-Extensions" };

			StreamCopyBufferSize = 81920;
		}
	}
}
