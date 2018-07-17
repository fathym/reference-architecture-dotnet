using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fathym
{
	[Serializable]
	[DataContract]
	public class Address
	{
		[DataMember]
		public virtual string City { get; set; }

		[DataMember]
		public virtual string Country { get; set; }

		[DataMember]
		public virtual string State { get; set; }

		[DataMember]
		public virtual string Street1 { get; set; }

		[DataMember]
		public virtual string Street2 { get; set; }

		[DataMember]
		public virtual string Unit { get; set; }

		[DataMember]
		public virtual string Zip { get; set; }
	}
}
