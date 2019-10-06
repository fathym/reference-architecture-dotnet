using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
	public static class HttpExtensions
	{
		public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
		{
			return await PatchAsync(client, new Uri(client.BaseAddress + requestUri), content);
		}

		public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content)
		{
			var request = new HttpRequestMessage
			{
				Method = new HttpMethod("PATCH"),
				RequestUri = requestUri,
				Content = content
			};

			return await client.SendAsync(request);
		}

		public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client,
			string requestUri, T value)
		{
			return await PatchAsJsonAsync<T>(client, new Uri(client.BaseAddress + requestUri), value);
		}

		public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client,
			Uri requestUri, T value)
		{
			var content = new ObjectContent<T>(value, new JsonMediaTypeFormatter(), "application/json");

			return await client.PatchAsync(requestUri, content);
		}
		
		public static async Task<T> ReadAsJSONAsync<T>(this HttpContent content)
		{
			var str = await content.ReadAsStringAsync();

			return str.FromJSON<T>();
		}

	}
}
