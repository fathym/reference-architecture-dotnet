using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Configuration
{
	public class FabricConfigurationManager : IConfigurationManager
	{
		#region Fields
		protected readonly ConfigurationSettings settings;
		#endregion

		#region Constructors
		public FabricConfigurationManager(ConfigurationSettings settings)
		{
			this.settings = settings;
		}
		#endregion

		#region API Methods
		public virtual T LoadConfigSetting<T>(string section, string name)
		{
			try
			{
				//FabricEventSource.Current.ServiceMessage(this, "Retrieving configuration setting for {0}:{1}",
				//    section, name);

				T value = default(T);

				var configSection = settings.Sections.FirstOrDefault(s => s.Name == section);

				if (configSection == null)
					FabricEventSource.Current.ConfigurationError(section, name, "Section was invalid");
				else
				{
					var parameter = configSection.Parameters.FirstOrDefault(p => p.Name == name);

					if (parameter == null || parameter.Value.IsNullOrEmpty())
						FabricEventSource.Current.ConfigurationError(section, name, "Config was invalid");
					else
					{
						value = parameter.Value.As<T>();

						//FabricEventSource.Current.ServiceMessage(this, 
						//    "Retrieved configuration setting {0}:{1} with value --> {2}", section, name, value);
					}
					//else
					//FabricEventSource.Current.ServiceMessage(this, "No configuration setting for {0}:{1}",
					//    section, name);
				}

				return value;
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
		}
		#endregion
	}
}
