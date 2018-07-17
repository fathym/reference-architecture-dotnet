using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Http;

namespace Fathym.Presentation.Proxy
{
	public class WebSocketProxyRequestHandler : IProxyRequestHandler
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;

		protected readonly ProxyOptions proxyOptions;
		#endregion

		#region Constructors
		public WebSocketProxyRequestHandler(ProxyOptions proxyOptions, IFabricAdapter fabricAdapter)
		{
			this.fabricAdapter = fabricAdapter;

			this.proxyOptions = proxyOptions;
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> Proxy(HttpContext context)
		{
			return await acceptProxyWebSocketRequest(context, proxyOptions);
		}
		#endregion

		#region Helpers
		protected virtual async Task<Status> acceptProxyWebSocketRequest(HttpContext context, ProxyOptions proxyOptions)
		{
			string destinationPath = proxyOptions.Proxy.Path + proxyOptions.Proxy.Query;

			//	TODO: Create new Client Communication facility like Http for WebSockets
			var destinationUri = new Uri("http://www.google.com").ToWebSocketScheme();

			using (var client = new ClientWebSocket())
			{
				foreach (var protocol in context.WebSockets.WebSocketRequestedProtocols)
					client.Options.AddSubProtocol(protocol);

				foreach (var headerEntry in context.Request.Headers)
					if (!proxyOptions.NotForwardedWebSocketHeaders.Contains(headerEntry.Key, StringComparer.OrdinalIgnoreCase))
						client.Options.SetRequestHeader(headerEntry.Key, headerEntry.Value);

				if (proxyOptions.WebSocketKeepAliveInterval.HasValue)
					client.Options.KeepAliveInterval = proxyOptions.WebSocketKeepAliveInterval.Value;

				try
				{
					await client.ConnectAsync(destinationUri, context.RequestAborted);
				}
				catch (WebSocketException wex)
				{
					context.Response.StatusCode = 400;

					return Status.GeneralError.Clone("Issue connecting WebSocket");
				}

				using (var server = await context.WebSockets.AcceptWebSocketAsync(client.SubProtocol))
				{
					var bufferSize = proxyOptions.WebSocketBufferSize;

					await Task.WhenAll(pumpWebSocket(client, server, bufferSize, context.RequestAborted), pumpWebSocket(server, client, bufferSize, context.RequestAborted));
				}

				return Status.Success;
			}
		}

		protected virtual async Task pumpWebSocket(WebSocket source, WebSocket destination, int bufferSize, CancellationToken cancellationToken)
		{
			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferSize));
			}

			var buffer = new byte[bufferSize];
			while (true)
			{
				WebSocketReceiveResult result;
				try
				{
					result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
				}
				catch (OperationCanceledException)
				{
					await destination.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null, cancellationToken);
					return;
				}
				if (result.MessageType == WebSocketMessageType.Close)
				{
					await destination.CloseOutputAsync(source.CloseStatus.Value, source.CloseStatusDescription, cancellationToken);
					return;
				}
				await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);
			}
		}
		#endregion
	}
}
