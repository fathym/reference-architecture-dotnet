using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Fluent
{
	public class ChainOfResponsibility<T> : IChain<T>, IChained<T>
	{
		#region Fields
		protected Func<Exception, bool> continueOnException;

		protected readonly List<Func<Task<T>>> responsibilities;

		protected Func<T, bool> shouldContinue;
		#endregion

		#region Constructors
		public ChainOfResponsibility()
		{
			continueOnException = (ex) => false;

			responsibilities = new List<Func<Task<T>>>();

			shouldContinue = (t) => t == null;
		}
		#endregion

		#region API Methods
		public virtual IChained<T> AddResponsibilities(params Func<T>[] responsibilities)
		{
			responsibilities.Each(r =>
			{
				AddResponsibility(r);
			});

			return this;
		}

		public virtual IChained<T> AddResponsibilities(params Func<Task<T>>[] responsibilities)
		{
			this.responsibilities.AddRange(responsibilities);

			return this;
		}

		public virtual IChained<T> AddResponsibility(Func<T> responsibility)
		{
			AddResponsibility(() => Task.FromResult(responsibility()));

			return this;
		}

		public virtual IChained<T> AddResponsibility(Func<Task<T>> responsibility)
		{
			AddResponsibilities(new[] { responsibility });

			return this;
		}

		public virtual async Task<T> Run()
		{
			T result = default(T);

			await responsibilities.Each(async r =>
			{
				bool cont;

				try
				{
					result = await r();

					cont = shouldContinue(result);
				}
				catch (Exception ex)
				{
					cont = continueOnException(ex);

					if (cont)
						result = default(T);
				}

				return !cont;
			});

			return result;
		}

		public virtual IChained<T> SetContinueOnException(Func<Exception, bool> continueOnException)
		{
			this.continueOnException = continueOnException;

			return this;
		}

		public virtual IChained<T> SetShouldContinue(Func<T, bool> shouldContinue)
		{
			this.shouldContinue = shouldContinue;

			return this;
		}
		#endregion
	}

	public interface IChain<T>
	{
		IChained<T> AddResponsibilities(params Func<T>[] responsibilities);

		IChained<T> AddResponsibilities(params Func<Task<T>>[] responsibilities);

		IChained<T> AddResponsibility(Func<T> responsibility);

		IChained<T> AddResponsibility(Func<Task<T>> responsibility);
	}

	public interface IChained<T>
	{
		IChained<T> AddResponsibilities(params Func<T>[] responsibilities);

		IChained<T> AddResponsibilities(params Func<Task<T>>[] responsibilities);

		IChained<T> AddResponsibility(Func<T> responsibility);

		IChained<T> AddResponsibility(Func<Task<T>> responsibility);

		Task<T> Run();

		IChained<T> SetContinueOnException(Func<Exception, bool> shouldContinue);

		IChained<T> SetShouldContinue(Func<T, bool> shouldContinue);
	}
}
