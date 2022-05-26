using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym
{
	public class Pageable<T>
	{
		public virtual IEnumerable<T> Items { get; set; }

		public virtual long TotalRecords { get; set; }

		#region Constructors
		public Pageable()
		{
			Items = Enumerable.Empty<T>();
		}
		#endregion
	}
}
