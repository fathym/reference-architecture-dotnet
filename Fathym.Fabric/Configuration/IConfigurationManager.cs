using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric.Configuration
{
	public interface IConfigurationManager
	{
		T LoadConfigSetting<T>(string section, string name);
	}
}
