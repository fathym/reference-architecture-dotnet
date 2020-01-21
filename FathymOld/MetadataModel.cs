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
}
