using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Fabric.Communications
{
	public class HttpCommunicationClientFactory : CommunicationClientFactoryBase<HttpCommunicationClient>
	{
		#region Fields
		protected readonly string baseApiAddress;
		#endregion

		#region Constructors
		public HttpCommunicationClientFactory(string baseApiAddress,
			ServicePartitionResolver servicePartitionResolver = null,
			IEnumerable<IExceptionHandler> exceptionHandlers = null,
			string traceId = null)
			: base(servicePartitionResolver, exceptionHandlers, traceId)
		{
			this.baseApiAddress = baseApiAddress;
		}
		#endregion

		#region Helpers
		protected override void AbortClient(HttpCommunicationClient client)
		{
			client.Abort();
		}

		protected override async Task<HttpCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
		{
			var httpClient = new HttpClient();

			if (!baseApiAddress.IsNullOrEmpty())
				httpClient.BaseAddress = new Uri(new Uri(endpoint.TrimEnd('/') + "/"), baseApiAddress.TrimEnd('/') + "/");
			else
				httpClient.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");

			return new HttpCommunicationClient(httpClient);
		}

		protected override bool ValidateClient(HttpCommunicationClient client)
		{
			return true;
		}

		protected override bool ValidateClient(string endpoint, HttpCommunicationClient client)
		{
			return client.ResolvedServicePartition.Endpoints.Select(e => e.Address).Contains(endpoint);
		}
		#endregion
	}
}
