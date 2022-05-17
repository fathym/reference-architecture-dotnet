using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Fathym
{
	[DataContract]
	public class MetadataModel
	{
		#region Properties
		[IgnoreDataMember]
		[JsonExtensionData]
		public virtual IDictionary<string, JsonNode> Metadata { get; set; }

		[DataMember(Name = "Metadata")]
		[JsonIgnore]
		public virtual string MetadataProxy
		{
			get
			{
				return Metadata.ToJSON();
			}
			set
			{
				Metadata = value.FromJSON<IDictionary<string, JsonNode>>();
			}
		}
		#endregion

		#region Constructors
		public MetadataModel()
		{
			Metadata = new Dictionary<string, JsonNode>();
		}
		#endregion
	}
}
