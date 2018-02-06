using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Runtime.Adapters
{
	public class StatelessFabricAdapter : GenericFabricAdapter<StatelessServiceContext>
	{
		#region Fields
		protected readonly StatelessServiceContext context;
		#endregion

		#region Constructors
		public StatelessFabricAdapter(StatelessServiceContext context)
			: base(context)
		{ }
		#endregion

		#region API Methods
		#endregion

		#region Helpers
		protected override dynamic resolveServiceListener(Func<dynamic, ICommunicationListener> createCommunicationListener)
		{
			return new ServiceInstanceListener(createCommunicationListener);
		}
		#endregion
	}
}
