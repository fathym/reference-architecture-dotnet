using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fathym
{
	[Serializable]
	[DataContract]
	public class Amount
	{
		#region Properties
		[DataMember]
		public virtual string Currency { get; set; }

		[DataMember]
		public virtual decimal Total { get; set; }
		#endregion

		#region Casts, Operators, Overrides
		public override bool Equals(object obj)
		{
			return obj is Amount && obj.As<Amount>() == this;
		}

		public override int GetHashCode()
		{
			var hash = 11;

			hash = (hash * 7) + Currency.GetHashCode();

			hash = (hash * 7) + Total.GetHashCode();

			return hash;
		}

		public static bool operator ==(Amount amount, Amount checkAmount)
		{
			bool areEqual = (object)amount == null && (object)checkAmount == null;

			if (!areEqual && amount != (object)null && checkAmount != (object)null)
				areEqual = amount.Total == checkAmount.Total && amount.Currency == checkAmount.Currency;

			return areEqual;
		}

		public static bool operator !=(Amount amount, Amount checkAmount)
		{
			return !(amount == checkAmount);
		}

		public static explicit operator decimal(Amount amount)
		{
			return amount.Total;
		}

		public static implicit operator Amount(decimal total)
		{
			return new Amount()
			{
				Currency = "USD",
				Total = total
			};
		}
		#endregion
	}
}
