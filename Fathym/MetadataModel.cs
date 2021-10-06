using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Fathym
{
	[DataContract]
	public class MetadataModel
	{
		#region Properties
		[IgnoreDataMember]
		[JsonExtensionData]
		public virtual IDictionary<string, JToken> Metadata { get; set; }

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
				Metadata = value.FromJSON<IDictionary<string, JToken>>();
			}
		}
		#endregion

		#region Constructors
		public MetadataModel()
		{
			Metadata = new Dictionary<string, JToken>();
		}
		#endregion
	}

	[DataContract]
	public class MetadataModelV2
	{
		#region Properties
		[IgnoreDataMember]
		[System.Text.Json.Serialization.JsonExtensionData]
		public virtual IDictionary<string, object> Metadata { get; set; }

		[DataMember(Name = "Metadata")]
		[System.Text.Json.Serialization.JsonIgnore]
		public virtual string MetadataProxy
		{
			get
			{
				return System.Text.Json.JsonSerializer.Serialize(Metadata);
			}
			set
			{
				Metadata = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object>>(value);
			}
		}
		#endregion

		#region Constructors
		public MetadataModelV2()
		{
			Metadata = new Dictionary<string, object>();
		}
		#endregion
	}
}
