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
		#endregion

		#region Constructors
		public HttpCommunicationClientFactory(ServicePartitionResolver servicePartitionResolver = null,
			IEnumerable<IExceptionHandler> exceptionHandlers = null,
			string traceId = null)
			: base(servicePartitionResolver, exceptionHandlers, traceId)
		{ }
		#endregion

		#region Helpers
		protected override void AbortClient(HttpCommunicationClient client)
		{
			client.Abort();
		}

		protected override async Task<HttpCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
		{
			var httpClient = new HttpClient();

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
