﻿using Fathym.Fabric.Runtime.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
		public virtual async Task<Status> Proxy(HttpContext context)
		{
			using (var requestMessage = createProxyHttpRequest(context))
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

		protected virtual HttpRequestMessage createProxyHttpRequest(HttpContext context)
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

			requestMessage.Method = new HttpMethod(request.Method);

			return requestMessage;
		}

		protected virtual async Task prepareRequest(HttpRequest request, HttpRequestMessage requestMessage)
		{
			if (proxyOptions.Proxy.Authorization != null)
				requestMessage.Headers.Authorization = proxyOptions.Proxy.Authorization;
		}

		protected virtual async Task<HttpResponseMessage> sendProxyHttpRequest(HttpContext context, ProxyOptions proxyOptions, HttpRequestMessage requestMessage)
		{
			HttpResponseMessage responseMessage = null;

			await withClient(proxyOptions,
				async (client) =>
				{
					var host = proxyOptions.Proxy.Host ?? client.BaseAddress.Host.Replace(Environment.MachineName.ToLower(), "localhost", StringComparison.InvariantCulture);

					var port = proxyOptions.Proxy.Port ?? client.BaseAddress.Port;

					var path = $"/{proxyOptions.Proxy.Path}".Replace("//", "/");

					var query = QueryString.FromUriComponent(proxyOptions.Proxy.Query ?? "?");

					var scheme = proxyOptions.Proxy.Scheme ?? client.BaseAddress.Scheme;

					var fullHost = new HostString(host, port);

					requestMessage.Headers.Host = fullHost.ToString();

					requestMessage.RequestUri = new Uri(UriHelper.BuildAbsolute(scheme, fullHost, path: path, query: query));

					client.Timeout = TimeSpan.FromMinutes(5);

					responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
				});

			return responseMessage;
		}

		protected virtual async Task withClient(ProxyOptions proxyOptions, Func<HttpClient, Task> action)
		{
			if (proxyOptions.Proxy.Connection.Application.StartsWith("@"))
			{
				var client = new HttpClient();

				client.BaseAddress = new Uri(proxyOptions.Proxy.Connection.Application.Substring(1));

				var parts = proxyOptions.Proxy.Connection.Service.Split('|');

				client.DefaultRequestHeaders.Add(parts[0], parts[1]);

				await action(client);
			}
			else
				await fabricAdapter.WithFabricClient(proxyOptions.Proxy.Connection.Application, proxyOptions.Proxy.Connection.Service, action);
		}
		#endregion
	}
}
