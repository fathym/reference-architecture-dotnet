using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Fabric.Runtime
{
	[Serializable]
	public class RuntimeContext
	{
		public virtual string ApplicationName { get; set; }

		public virtual string ServiceName { get; set; }
	}
}
