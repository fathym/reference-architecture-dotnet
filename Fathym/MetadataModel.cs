using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Fathym
{
	//[DataContract]
	//public class MetadataModel
	//{
	//	#region Properties
	//	[IgnoreDataMember]
	//	[JsonExtensionData]
	//	public virtual Dictionary<string, object> Metadata { get; set; }

	//	[DataMember(Name = "Metadata")]
	//	[JsonIgnore]
	//	public virtual string MetadataProxy
	//	{
	//		get
	//		{
	//			return Metadata.ToJSON();
	//		}
	//		set
	//		{
	//			Metadata = value.FromJSON<JsonObject>();
	//		}
	//	}
	//	#endregion

	//	#region Constructors
	//	public MetadataModel()
	//	{
	//		Metadata = new JsonObject();
	//	}
	//	#endregion
	//}
}
