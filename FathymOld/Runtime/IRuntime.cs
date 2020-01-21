using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Runtime
{
	public interface IRuntime
	{
		Task Cycle(CancellationToken cancellationToken);

		Task Startup(CancellationToken cancellationToken);
	}
}
