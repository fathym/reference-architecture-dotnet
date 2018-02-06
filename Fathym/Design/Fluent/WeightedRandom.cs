using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Fluent
{
    public class WeightedRandom<T> : IWeightedRandom<T>, IWeightedRandomly<T>
	{
		#region Fields
		protected int factor;

		protected double step;

		protected IDictionary<T, double> weightRanges;
		#endregion

		#region Constructors
		public WeightedRandom()
		{
			factor = 10;

			step = .1;

			weightRanges = new Dictionary<T, double>();
		}
		#endregion

		#region API Methods
		public virtual IWeightedRandomly<T> AddWeightedLookup(T value, double weight)
		{
			weightRanges.Add(value, weight);

			return this;
		}

		public virtual IWeightedRandomly<T> AddWeightedLookups(IDictionary<T, double> weightRanges)
		{
			weightRanges.ForEach(wr => weightRanges.Add(wr));

			return this;
		}

		public virtual T Run()
		{
			var randGen = new Random();

			if (weightRanges.IsNullOrEmpty())
				return default(T);

			var totalweight = (int)Math.Floor(weightRanges.Sum(wr => wr.Value));

			var choice = randGen.Next(totalweight * factor) / (double)factor;

			if (choice > totalweight)
				throw new Exception("The choice is greater than total weight.");

			double sum = 0;

			T retValue = weightRanges.First().Key;

			weightRanges.ForEach(
				(wr) =>
				{
					for (double i = sum; i < wr.Value + sum; i += step)
					{
						if (i >= choice)
						{
							retValue = wr.Key;

							return true;
						}
					}

					sum += wr.Value;

					return false;
				});

			return retValue;
		}
		
		public virtual IWeightedRandomly<T> SetFactor(int factor)
		{
			this.factor = factor;

			return this;
		}

		public virtual IWeightedRandomly<T> SetStep(double step)
		{
			this.step = step;

			return this;
		}
		#endregion
	}

	public interface IWeightedRandom<T>
	{
		IWeightedRandomly<T> AddWeightedLookup(T value, double weight);

		IWeightedRandomly<T> AddWeightedLookups(IDictionary<T, double> weightRanges);
	}

	public interface IWeightedRandomly<T>
	{
		IWeightedRandomly<T> AddWeightedLookup(T value, double weight);

		IWeightedRandomly<T> AddWeightedLookups(IDictionary<T, double> weightRanges);

		IWeightedRandomly<T> SetFactor(int factor);

		IWeightedRandomly<T> SetStep(double step);

		T Run();
	}
}
