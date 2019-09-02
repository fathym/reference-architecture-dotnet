using Fathym.API;
using Fathym.API.Fluent;
using Fathym.Design;
using Fathym.Fabric.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.API.Controllers
{
	public abstract class FathymAPIController : ControllerBase
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
		protected virtual IAPIBoundaried<T> withAPIBoundary<T>(Func<T, Task<T>> action)
			 where T : BaseResponse, new()
		{
			return withAPIBoundaried<T>(api =>
			{
				return api.SetAction(action);
			});
		}

		protected virtual IAPIBoundaried<T> withAPIBoundaried<T>(Func<IAPIBoundary<T>, IAPIBoundaried<T>> api)
			 where T : BaseResponse, new()
		{
			return api(withAPIBoundary<T>());
		}

		protected virtual IAPIBoundary<BaseResponse<T>> withModeledAPIBoundary<T>()
		{
			return new APIBoundary<BaseResponse<T>>();
		}

		protected virtual void loadApiKey(out string apiKey, out string scheme)
		{
			var authHeader = Request.Headers["Authorization"];

			if (!authHeader.IsNullOrEmpty())
			{
				var parts = authHeader.ToString().Split(' ');

				apiKey = parts[1].Base64Decode();

				scheme = parts[0];
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
