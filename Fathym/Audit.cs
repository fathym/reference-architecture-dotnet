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
	public class Audit : MetadataModel
	{
		#region Constructors
		public Audit()
		{
			At = DateTime.UtcNow;
		}
		#endregion

		#region Properties
		[DataMember]
		public virtual DateTime At { get; set; }
		
		[DataMember]
		public virtual string By { get; set; }

		[DataMember]
		public virtual string Description { get; set; }
		#endregion
	}
}
