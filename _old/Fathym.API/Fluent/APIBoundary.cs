using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.API.Fluent
{
	public class APIBoundary<T> : IAPIBoundary<T>, IAPIBoundaried<T>, IAPIBoundaryWithDefault<T>
		where T : BaseResponse, new()
	{
		#region Fields
		protected Func<T, Task<T>> action;

		protected T defaultResponse;

		protected Func<Exception, T, Task<T>> excceptionHandle;
		#endregion

		#region Constructors
		public APIBoundary()
		{
			excceptionHandle = (ex, resp) =>
			{
				resp.Status = Status.GeneralError.Clone(ex.ToString());

				return Task.FromResult(resp);
			};
		}
		#endregion

		#region API Methods
		public virtual async Task<T> Run()
		{
			try
			{
				if (defaultResponse == null)
					defaultResponse = new T();

				defaultResponse = await action(defaultResponse);
			}
			catch (Exception ex)
			{
				//FabricEventSource.Current.ServiceRequestFailed(ActionContext.ActionDescriptor.ActionName, ex.ToString());

				if (defaultResponse == null)
					defaultResponse = new T();

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

		//public virtual IAPIBoundary<BaseResponse<TModel>> WithModel<TModel>()
		//{
		//	return new ModeledAPIBoundary<T>();
		//}
		#endregion

		#region Helpers

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
