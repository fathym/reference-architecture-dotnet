using Fathym.API;
using Fathym.Design;
using Fathym.Fabric.Runtime;
using Fathym.Fabric.Runtime.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Fathym.Fabric.API.Controllers
{
	[RoutePrefix("vx/5280/ping")]
	public class PingAPIController : FabricAPIController
	{
		#region Constructors
		public PingAPIController(IFabricAdapter adapter)
			: base(adapter)
		{ }
		#endregion

		#region API Methods
		[HttpDelete]
		[Route("")]
		public virtual async Task<BaseResponse> Delete(int code)
		{
			return await withAPIBoundary<BaseResponse>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Status = new Status()
					{
						Code = code,
						Message = "Ping Requested Status",
						MetadataProxy = new
						{
							On = DateTime.UtcNow
						}.ToJSON()
					};

					return response;
				}).Run();
		}

		[HttpGet]
		[Route("")]
		public virtual async Task<BaseResponse> Get(int code)
		{
			return await withAPIBoundary<BaseResponse>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Status = new Status()
					{
						Code = code,
						Message = "Ping Requested Status",
						MetadataProxy = new
						{
							On = DateTime.UtcNow
						}.ToJSON()
					};

					return response;
				}).Run();
		}

		[HttpPatch]
		[Route("")]
		public virtual async Task<BaseResponse<dynamic>> Patch(dynamic data, [FromUri]int code)
		{
			return await withAPIBoundary<BaseResponse<dynamic>>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Model = data;

					response.Status = new Status()
					{
						Code = code,
						Message = "Ping Requested Status",
						MetadataProxy = new
						{
							On = DateTime.UtcNow
						}.ToJSON()
					};

					return response;
				}).Run();
		}

		[HttpPost]
		[Route("")]
		public virtual async Task<BaseResponse<dynamic>> Post(dynamic data, [FromUri]int code)
		{
			return await withAPIBoundary<BaseResponse<dynamic>>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Model = data;

					response.Status = new Status()
					{
						Code = code,
						Message = "Ping Requested Status",
						MetadataProxy = new
						{
							On = DateTime.UtcNow
						}.ToJSON()
					};

					return response;
				}).Run();
		}

		[HttpPut]
		[Route("")]
		public virtual async Task<BaseResponse<dynamic>> Put(dynamic data, [FromUri]int code)
		{
			return await withAPIBoundary<BaseResponse<dynamic>>()
				.SetAction(async (response) =>
				{
					await DesignOutline.Instance.Async().Delay().Run();

					response.Model = data;

					response.Status = new Status()
					{
						Code = code,
						Message = "Ping Requested Status",
						MetadataProxy = new
						{
							On = DateTime.UtcNow
						}.ToJSON()
					};

					return response;
				}).Run();
		}
		#endregion

		#region Helpers
		#endregion
	}
}
