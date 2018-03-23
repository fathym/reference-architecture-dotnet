using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Http;

namespace Fathym.Presentation.Proxy
{
	public class HttpClientProxyRequestHandler : IProxyRequestHandler
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;

		protected readonly ProxyOptions proxyOptions;
		#endregion

		#region Constructors
		public HttpClientProxyRequestHandler(ProxyOptions proxyOptions, IFabricAdapter fabricAdapter)
		{
			this.fabricAdapter = fabricAdapter;

			this.proxyOptions = proxyOptions;
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> Proxy(HttpContext context, Uri destinationUri)
		{
			using (var requestMessage = createProxyHttpRequest(context, destinationUri))
			{
				await prepareRequest(context.Request, requestMessage);

				using (var responseMessage = await sendProxyHttpRequest(context, proxyOptions, requestMessage))
					await copyProxyHttpResponse(context, proxyOptions, responseMessage);

				return Status.Success;
			}
		}
		#endregion

		#region Helpers
		protected virtual async Task copyProxyHttpResponse(HttpContext context, ProxyOptions proxyOptions, HttpResponseMessage responseMessage)
		{
			var response = context.Response;

			response.StatusCode = (int)responseMessage.StatusCode;

			foreach (var header in responseMessage.Headers)
				response.Headers[header.Key] = header.Value.ToArray();

			foreach (var header in responseMessage.Content.Headers)
				response.Headers[header.Key] = header.Value.ToArray();

			// SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
			response.Headers.Remove("transfer-encoding");

			using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
				await responseStream.CopyToAsync(response.Body, proxyOptions.StreamCopyBufferSize, context.RequestAborted);
		}

		protected virtual HttpRequestMessage createProxyHttpRequest(HttpContext context, Uri uri)
		{
			var request = context.Request;

			var requestMessage = new HttpRequestMessage();
			var requestMethod = request.Method;
			if (!HttpMethods.IsGet(requestMethod) &&
				!HttpMethods.IsHead(requestMethod) &&
				!HttpMethods.IsDelete(requestMethod) &&
				!HttpMethods.IsTrace(requestMethod))
			{
				var streamContent = new StreamContent(request.Body);

				requestMessage.Content = streamContent;
			}

			// Copy the request headers
			foreach (var header in request.Headers)
			{
				if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
					requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
			}

			requestMessage.Headers.Host = uri.Authority;

			requestMessage.RequestUri = uri;

			requestMessage.Method = new HttpMethod(request.Method);

			return requestMessage;
		}

		protected virtual async Task prepareRequest(HttpRequest request, HttpRequestMessage requestMessage)
		{
			requestMessage.Headers.Authorization = proxyOptions.Authorization;
		}

		protected virtual async Task<HttpResponseMessage> sendProxyHttpRequest(HttpContext context, ProxyOptions proxyOptions, HttpRequestMessage requestMessage)
		{
			HttpResponseMessage responseMessage = null;

			await withClient(proxyOptions,
				async (client) =>
				{
					responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
				});

			return responseMessage;
		}

		protected virtual async Task withClient(ProxyOptions proxyOptions, Func<HttpClient, Task> action)
		{
			await fabricAdapter.WithFabricClient(proxyOptions.Proxy.Application, proxyOptions.Proxy.Service, action);
		}
		#endregion
	}
}
