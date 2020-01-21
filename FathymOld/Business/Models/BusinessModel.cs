using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Business.Models
{
	[DataContract]
	public class BusinessModel<TID> : MetadataModel
	{
		#region Properties
		[DataMember]
		public virtual Audit Created { get; set; }

		[DataMember]
		public virtual TID ID { get; set; }

		[DataMember]
		public virtual Audit Modified { get; set; }
		#endregion
	}
}
