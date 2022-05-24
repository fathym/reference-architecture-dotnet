using System;
using System.Runtime.Serialization;

namespace Fathym
{
	[Serializable]
	[DataContract]
	public class Status : MetadataModel
	{
		#region Constants
		public static Status Success { get { return new Status() { Code = (int)StatusCodeTypes.Success, Message = "Success" }; } }

		public static Status GeneralError { get { return new Status() { Code = (int)StatusCodeTypes.GeneralError, Message = "General Error" }; } }

		public static Status Initialized { get { return new Status() { Code = (int)StatusCodeTypes.Initialized, Message = "Initialized" }; } }

		public static Status Conflict { get { return new Status() { Code = (int)StatusCodeTypes.Conflict, Message = "Conflict" }; } }

		public static Status NotLocated { get { return new Status() { Code = (int)StatusCodeTypes.NotLocated, Message = "Not Located" }; } }

		public static Status Unauthorized { get { return new Status() { Code = (int)StatusCodeTypes.Unauthorized, Message = "Unauthorized" }; } }
		#endregion

		#region Properties
		[DataMember]
		public virtual long Code { get; set; } 

		[DataMember]
		public virtual string Message { get; set; }
		#endregion

		#region Constructors
		public Status()
		{ }
		#endregion

		#region API Methods
		public virtual Status Clone()
		{
			return Clone(Message);
		}

		public virtual Status Clone(string message, dynamic metadata = null)
		{
			var status = new Status() { Code = Code, Message = message };

			//status.MetadataProxy = ((object)metadata).ToJSON();

			return status;
		}
		#endregion

		#region Casts, Operators, Overrides
		public override bool Equals(object obj)
		{
			return obj is Status && obj.As<Status>() == this;
		}

		public override int GetHashCode()
		{
			var hash = 11;

			hash = (hash * 7) + Code.GetHashCode();

			hash = (hash * 7) + Message.GetHashCode();

			hash = (hash * 7) + Metadata.GetHashCode();

			return hash;
		}

		public static bool operator ==(Status status, Status checkStatus)
		{
			bool areEqual = (object)status == null && (object)checkStatus == null;

			if (!areEqual && (object)status != null && (object)checkStatus != null)
				areEqual = status.Code == checkStatus.Code;

			return areEqual;
		}

		public static bool operator !=(Status status, Status checkStatus)
		{
			return !(status == checkStatus);
		}

		public static implicit operator bool(Status status)
		{
			return status == null ? false : status == Success;
		}

		public static implicit operator Status(bool status)
		{
			return status ? Success : GeneralError;
		}
		#endregion
	}

	[Serializable]
	[DataContract]
	public enum StatusCodeTypes : long
	{
		[EnumMember]
		Initialized = -1,

		[EnumMember]
		Success = 0,

		[EnumMember]
		GeneralError = 1,

		[EnumMember]
		NotLocated = 2,

		[EnumMember]
		Conflict = 3,

		[EnumMember]
		Unauthorized = 4
	}
}
