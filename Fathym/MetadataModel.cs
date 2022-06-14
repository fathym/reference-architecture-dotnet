using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Fathym
{
	public class MetadataModel
	{
		[JsonExtensionData]
		[Newtonsoft.Json.JsonExtensionData]
		public virtual Dictionary<string, object> Metadata { get; set; }

        #region Constructors
        public MetadataModel()
        {
            Metadata = new Dictionary<string, object>();
        }
        #endregion
    }
}
