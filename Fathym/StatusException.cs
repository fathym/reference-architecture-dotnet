using System;

namespace Fathym
{
	public class StatusException : Exception
	{
		#region Properties
		public virtual Status Status { get; set; }
		#endregion

		#region Constructors
		public StatusException()
			: base()
		{ }

		public StatusException(Status status)
			: base(status.Message)
		{
			Status = status;
		}

		public StatusException(Status status, Exception ex)
			: base(status.Message, ex)
		{
			Status = status;
		}
		#endregion
	}
}
