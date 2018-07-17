using Fathym.Fabric.Communications;
using Fathym.Fabric.Configuration;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.API.Controllers
{
	public abstract class FabricAPIController : FathymAPIController
	{
		#region Fields
		protected IFabricAdapter fabricAdapter;
		#endregion

		#region Constructors
		public FabricAPIController(IFabricAdapter fabricAdapter)
		{
			this.fabricAdapter = fabricAdapter;
		}
		#endregion

		#region Helpers
		protected virtual T loadConfigSetting<T>(string section, string name)
		{
			var config = fabricAdapter.GetConfiguration();

			return config.LoadConfigSetting<T>(section, name);
		}

		protected virtual T loadConfigSetting<T>(string name)
		{
			return loadConfigSetting<T>(GetType().FullName, name);
		}
		#endregion
	}
}
