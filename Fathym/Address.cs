using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym
{
	public class Address
	{
		public virtual string City { get; set; }

		public virtual string Country { get; set; }

		public virtual string State { get; set; }

		public virtual string Street1 { get; set; }

		public virtual string Street2 { get; set; }

		public virtual string Unit { get; set; }

		public virtual string Zip { get; set; }
	}
}
