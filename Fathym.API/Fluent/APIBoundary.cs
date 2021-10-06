using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.API.Fluent
{
    public class APIResponseBoundary<T> : APIBoundary<T>
        where T : BaseResponse, new()
    {
        #region Constructors
        public APIResponseBoundary(ILogger logger)
            : base(logger)
        { }
        #endregion

        #region Helpers
        protected override Func<Exception, T, Task<T>> createDefaultExceptionHandle()
        {
            return (ex, resp) =>
            {

                resp.Status = Status.GeneralError.Clone(ex.ToString());

                return Task.FromResult(resp);
            };
        }

        protected override T getDefaultResponse()
        {
            return new T();
        }
        #endregion
    }

    public class APIBoundary<T> : IAPIBoundary<T>, IAPIBoundaried<T>, IAPIBoundaryWithDefault<T>
    {
        #region Fields
        protected Func<T, Task<T>> action;

        protected T defaultResponse;

        protected Func<Exception, T, Task<T>> excceptionHandle;

        protected readonly ILogger logger;
        #endregion

        #region Constructors
        public APIBoundary(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            excceptionHandle = createDefaultExceptionHandle();
        }
        #endregion

        #region API Methods
        public virtual async Task<T> Run()
        {
            try
            {
                if (defaultResponse == null)
                    defaultResponse = getDefaultResponse();

                defaultResponse = await action(defaultResponse);
            }
            catch (Exception ex)
            {
                //FabricEventSource.Current.ServiceRequestFailed(ActionContext.ActionDescriptor.ActionName, ex.ToString());

                if (defaultResponse == null)
                    defaultResponse = getDefaultResponse();

                if (excceptionHandle != null)
                    defaultResponse = await excceptionHandle(ex, defaultResponse);
            }

            return defaultResponse;
        }

        public virtual IAPIBoundaried<T> SetAction(Func<T, Task<T>> action)
        {
            this.action = action;

            return this;
        }

        public virtual IAPIBoundaryWithDefault<T> SetDefaultResponse(T response)
        {
            defaultResponse = response;

            return this;
        }

        public virtual IAPIBoundaried<T> SetExceptionHandler(Func<Exception, T, Task<T>> excceptionHandler)
        {
            this.excceptionHandle = excceptionHandler;

            return this;
        }
        #endregion

        #region Helpers
        protected virtual Func<Exception, T, Task<T>> createDefaultExceptionHandle()
        {
            return (ex, resp) =>
            {
                logger.LogError(ex, $"There was an exception in the API boundary for {GetType().FullName}");

                throw ex;
            };
        }

        protected virtual T getDefaultResponse()
        {
            return default(T);
        }
        #endregion
    }

    public interface IAPIBoundary<T>
    {
        IAPIBoundaryWithDefault<T> SetDefaultResponse(T response);

        IAPIBoundaried<T> SetAction(Func<T, Task<T>> action);

        //IAPIBoundary<T> WithModel<TModel>();
    }

    public interface IAPIBoundaryWithDefault<T>
    {
        IAPIBoundaried<T> SetAction(Func<T, Task<T>> action);
    }

    public interface IAPIBoundaried<T>
    {
        IAPIBoundaried<T> SetExceptionHandler(Func<Exception, T, Task<T>> excceptionHandler);

        Task<T> Run();
    }

}
