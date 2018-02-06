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
	public class StatefulFabricAdapter : GenericFabricAdapter<StatefulServiceContext>
	{
		#region Fields
		protected readonly StatefulServiceContext context;
		#endregion

		#region Constructors
		public StatefulFabricAdapter(StatefulServiceContext context)
			: base(context)
		{ }
		#endregion

		#region API Methods
		#endregion

		#region Helpers
		protected override dynamic resolveServiceListener(Func<dynamic, ICommunicationListener> createCommunicationListener)
		{
			return new ServiceReplicaListener(createCommunicationListener);
		}
		#endregion
	}
}
