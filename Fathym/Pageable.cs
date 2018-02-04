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
	public class Pageable<T>
	{
		[DataMember]
		public virtual IEnumerable<T> Items { get; set; }

		[DataMember]
		public virtual long TotalRecords { get; set; }

		#region Constructors
		public Pageable()
		{
			Items = Enumerable.Empty<T>();
		}
		#endregion
	}
}
