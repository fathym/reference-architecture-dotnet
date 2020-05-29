using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design.Fluent
{
    public class Retry : IRetry, IRetried
	{
		#region Fields
		protected Func<Task<bool>> action;

		protected int cycles;

		protected Action failureHandle;

		protected int throttle;

		protected double throttleScale;
		#endregion

		#region Constructors
		public Retry()
		{
			cycles = 3;

			throttle = 1000;

			throttleScale = 1;
		}
		#endregion

		#region API Methods
		public virtual async Task Run()
		{
			if (cycles < 1)
				cycles = 1;

			int currentCycle = 1;

			bool shouldRetry = false;

			while (action != null && currentCycle <= cycles)
			{
				shouldRetry = await action();

				if (shouldRetry)
				{
					await Task.Delay(throttle);

					throttle = (int)Math.Ceiling(throttleScale * throttle);

					currentCycle++;
				}
				else
					break;
			}

			if (failureHandle != null && shouldRetry)
				failureHandle();
		}

		public virtual IRetried SetAction(Func<bool> action)
		{
			this.action = () => Task.FromResult(action());

			return this;
		}

		public virtual IRetried SetActionAsync(Func<Task<bool>> action)
		{
			this.action = action;

			return this;
		}

		public virtual IRetried SetCycles(int cycles)
		{
			this.cycles = cycles;

			return this;
		}

		public virtual IRetried SetFailureHandle(Action failureHandle)
		{
			this.failureHandle = failureHandle;

			return this;
		}

		public virtual IRetried SetThrottle(int throttle)
		{
			this.throttle = throttle;

			return this;
		}

		public virtual IRetried SetThrottleScale(double throttleScale)
		{
			this.throttleScale = throttleScale;

			return this;
		}
		#endregion
	}

	public interface IRetry
	{
		IRetried SetAction(Func<bool> action);

		IRetried SetActionAsync(Func<Task<bool>> action);
	}

	public interface IRetried
	{
		IRetried SetCycles(int cycles);

		IRetried SetFailureHandle(Action failureHandle);

		IRetried SetThrottle(int throttle);

		IRetried SetThrottleScale(double throttleScale);

		Task Run();
	}
}
