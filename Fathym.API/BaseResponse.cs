using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.API
{
	public class BaseResponse
	{
		public virtual Status Status { get; set; }
	}

	public class BaseResponse<TModel> : BaseResponse
	{
		public virtual TModel Model { get; set; }
	}
}
