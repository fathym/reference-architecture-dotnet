using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Communications
{
	public class HttpCommunicationClient : ICommunicationClient
	{
		#region Fields
		#endregion

		#region Properties
		public virtual ResolvedServiceEndpoint Endpoint { get; set; }

		public virtual HttpClient HttpClient { get; protected set; }

		public string ListenerName { get; set; }

		public virtual ResolvedServicePartition ResolvedServicePartition { get; set; }
		#endregion

		#region Constructors
		public HttpCommunicationClient(HttpClient httpClient)
		{
			this.HttpClient = httpClient;
		}
		#endregion

		#region API methods
		public virtual void Abort()
		{
			HttpClient.Dispose();
		}

		public virtual async Task<TResult> Delete<TResult>(string requestUri)
		{
			return await Delete<TResult>(new Uri(HttpClient.BaseAddress, requestUri));
		}

		public virtual async Task<TResult> Delete<TResult>(Uri requestUri)
		{
			try
			{
				var result = await HttpClient.DeleteAsync(requestUri);

				var resultBody = await result.Content.ReadAsStringAsync();

				if (resultBody.StartsWith("<"))
					throw new Exception($"Error in request to {requestUri}: {resultBody}");

				return resultBody.FromJSON<TResult>();
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}

		public virtual async Task<TResult> Get<TResult>(string requestUri)
		{
			return await Get<TResult>(new Uri(HttpClient.BaseAddress, requestUri));
		}

		public virtual async Task<TResult> Get<TResult>(Uri requestUri)
		{
			try
			{
				var result = await HttpClient.GetStringAsync(requestUri);

				if (result.StartsWith("<"))
					throw new Exception($"Error in request to {requestUri}: {result}");

				return result.FromJSON<TResult>();
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}

		public virtual async Task<TResult> Patch<TModel, TResult>(string requestUri, TModel model)
		{
			return await Patch<TModel, TResult>(new Uri(HttpClient.BaseAddress, requestUri), model);
		}

		public virtual async Task<TResult> Patch<TModel, TResult>(Uri requestUri, TModel model)
		{
			try
			{
				var result = await HttpClient.PatchAsJsonAsync(requestUri, model);

				var resultBody = await result.Content.ReadAsStringAsync();

				if (resultBody.StartsWith("<"))
					throw new Exception($"Error in request to {requestUri}: {resultBody}");

				return resultBody.FromJSON<TResult>();
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}

		public virtual async Task<TResult> Post<TModel, TResult>(string requestUri, TModel model)
		{
			return await Post<TModel, TResult>(new Uri(HttpClient.BaseAddress, requestUri), model);
		}

		public virtual async Task<TResult> Post<TModel, TResult>(Uri requestUri, TModel model)
		{
			try
			{
				var result = await HttpClient.PostAsJsonAsync(requestUri, model);

				var resultBody = await result.Content.ReadAsStringAsync();

				if (resultBody.StartsWith("<"))
					throw new Exception($"Error in request to {requestUri}: {resultBody}");

				return resultBody.FromJSON<TResult>();
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}

		public virtual async Task<TResult> Put<TModel, TResult>(string requestUri, TModel model)
		{
			return await Put<TModel, TResult>(new Uri(HttpClient.BaseAddress, requestUri), model);
		}

		public virtual async Task<TResult> Put<TModel, TResult>(Uri requestUri, TModel model)
		{
			try
			{
				var result = await HttpClient.PutAsJsonAsync(requestUri, model);

				var resultBody = await result.Content.ReadAsStringAsync();

				if (resultBody.StartsWith("<"))
					throw new Exception($"Error in request to {requestUri}: {resultBody}");

				return resultBody.FromJSON<TResult>();
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}
		#endregion
	}
}
