using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.API
{
	[DataContract]
	public class BaseResponse
	{
		[DataMember]
		public virtual Status Status { get; set; }
	}

	[DataContract]
	public class BaseResponse<TModel> : BaseResponse
	{
		[DataMember]
		public virtual TModel Model { get; set; }
	}
}
