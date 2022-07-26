﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fathym.Business.Models
{
	public class BusinessModel<TID> : MetadataModel
	{
		#region Properties
		public virtual Audit Created { get; set; }

		public virtual TID ID { get; set; }

		public virtual Audit Modified { get; set; }
		#endregion
	}
}
