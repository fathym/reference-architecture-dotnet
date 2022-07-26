using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq;
using System.Text.Json;

namespace Fathym
{
    public class MetadataModel
    {
        #region Fields
        protected Dictionary<string, object> metadata;
        #endregion

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
