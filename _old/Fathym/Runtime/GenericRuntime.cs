using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Runtime
{
	public abstract class GenericRuntime : IRuntime
	{
		#region Constructors
		public GenericRuntime()
		{ }
		#endregion

		#region API Methods
		public abstract Task Cycle(CancellationToken cancellationToken);

		public abstract Task Startup(CancellationToken cancellationToken);
		#endregion
	}
}
