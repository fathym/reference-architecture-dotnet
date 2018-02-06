using Fathym.API;
using Fathym.API.Fluent;
using Fathym.Design;
using Fathym.Fabric.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Fathym.Fabric.API.Controllers
{
	public abstract class Fifty280APIController : ApiController
	{
		#region API Methods
		[Route("status", Order = 1)]
		[HttpGet, HttpPost]
		public virtual async Task<BaseResponse> LoadStatus()
		{
			return await withAPIBoundary<BaseResponse>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Status = Status.Success;

					return response;
				}).Run();
		}
		#endregion

		#region Helpers
		protected virtual IAPIBoundary<T> withAPIBoundary<T>()
			where T : BaseResponse, new()
		{
			return new APIBoundary<T>();
		}

		protected virtual IAPIBoundary<BaseResponse<T>> withModeledAPIBoundary<T>()
		{
			return new APIBoundary<BaseResponse<T>>();
		}

		protected virtual void loadApiKey(out string apiKey, out string scheme)
		{
			var authHeader = Request.Headers.Authorization;

			if (authHeader != null)
			{
				apiKey = authHeader.Parameter.Base64Decode();

				scheme = authHeader.Scheme;
			}
			else
			{
				apiKey = null;

				scheme = null;
			}
		}

		protected virtual void loadApiCredentials(out string username, out string password, char split = '|')
		{
			string apiKey;
			string scheme;

			loadApiKey(out apiKey, out scheme);

			if (!apiKey.IsNullOrEmpty())
			{
				var parts = apiKey.Split(split);

				username = parts[0];

				password = parts[1];
			}
			else
			{
				username = null;

				password = null;
			}
		}
		#endregion
	}
}
