using Fathym.API;
using Fathym.API.Fluent;
using Fathym.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.API.Controllers
{
    public abstract class FathymAPIController : ControllerBase
    {
        #region Fields
        protected readonly ILogger logger;
        #endregion

        #region Constructors
        public FathymAPIController(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion

        #region API Methods
        [Route("status", Order = 1000)]
        [HttpGet, HttpPost]
        public virtual async Task<BaseResponse> LoadStatus()
        {
            return await withAPIBoundary<BaseResponse>()
                .SetAction(async (response) =>
                {
                    await DesignOutline.Instance.Async()
                        .Delay().Run();

                    response.Status = Status.Success;

                    return response;
                }).Run();
        }
        #endregion

        #region Helpers
        protected virtual IAPIBoundary<T> withAPIBoundary<T>()
            where T : BaseResponse, new()
        {
            return new APIResponseBoundary<T>(logger);
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
            return new APIResponseBoundary<BaseResponse<T>>(logger);
        }
        #endregion
    }
}
