﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fathym
{
	public class Audit : MetadataModel
	{
		#region Properties
		public virtual DateTime At { get; set; }

		public virtual string By { get; set; }

		public virtual string Description { get; set; }

		public virtual string Details { get; set; }
		#endregion

		#region Constructors
		public Audit()
		{
			At = DateTime.UtcNow;
		}
		#endregion

	}
}
