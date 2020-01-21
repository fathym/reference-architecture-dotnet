using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Design.Fluent
{
	public class Async : IAsync, IAsyncDelay, IAsyncDelayed, IAsyncQueue, IAsyncQueued, IAsyncPulse, IAsyncPulsed
	{
		#region Fields
		protected TimeSpan delayWait;

		protected AsyncMode mode;

		protected Func<object, Status> pulseAction;

		protected Action<object> queueAction;

		protected object state;
		#endregion

		#region Constructors
		public Async()
		{
			delayWait = TimeSpan.FromMilliseconds(1000);
		}
		#endregion

		#region API Methods
		public virtual IAsyncDelay Delay()
		{
			mode = AsyncMode.Delay;

			return this;
		}

		public virtual IAsyncPulse Pulse(object state)
		{
			mode = AsyncMode.Pulse;

			this.state = state;

			return this;
		}

		public virtual IAsyncQueue Queue(object state)
		{
			mode = AsyncMode.Queue;

			this.state = state;

			return this;
		}

		public virtual async Task Run()
		{
			switch (mode)
			{
				case AsyncMode.Delay:
					await Task.Delay(delayWait);
					break;

				case AsyncMode.Pulse:
					dynamic pulseState = new ExpandoObject();

					pulseState.Interval = delayWait;

					pulseState.Action = pulseAction;

					pulseState.State = state;

					ThreadPool.QueueUserWorkItem((s) =>
					{
						dynamic sd = (ExpandoObject)s;

						var runPulse = Status.Success;

						while (runPulse)
						{
							runPulse = sd.Action(sd.State);

							if (runPulse)
								Task.Delay(sd.Interval);
						}
					}, (object)pulseState);
					break;

				case AsyncMode.Queue:
					ThreadPool.QueueUserWorkItem(s => queueAction(s), state);
					break;
			}
		}

		#region Delay
		public virtual IAsyncDelayed SetDelay(TimeSpan wait)
		{
			delayWait = wait;

			return this;
		}

		public virtual IAsyncDelayed SetMillisecondDelay(int wait)
		{
			return SetDelay(TimeSpan.FromMilliseconds(wait));
		}
		#endregion

		#region Pulse
		public IAsyncPulsed SetAction(Func<object, Status> action)
		{
			pulseAction = action;

			return this;
		}

		public virtual IAsyncPulsed SetInterval(TimeSpan interval)
		{
			delayWait = interval;

			return this;
		}
		#endregion

		#region Queue
		public IAsyncQueued SetAction(Action<object> action)
		{
			queueAction = action;

			return this;
		}
		#endregion
		#endregion
	}

	public enum AsyncMode
	{
		Delay,
		Pulse,
		Queue
	}

	public interface IAsync
	{
		IAsyncDelay Delay();

		IAsyncPulse Pulse(object state);

		IAsyncQueue Queue(object state);
	}

	public interface IAsyncDelay
	{
		IAsyncDelayed SetDelay(TimeSpan wait);

		IAsyncDelayed SetMillisecondDelay(int wait);

		Task Run();
	}

	public interface IAsyncDelayed
	{
		Task Run();
	}

	public interface IAsyncQueue
	{
		IAsyncQueued SetAction(Action<object> action);
	}

	public interface IAsyncQueued
	{
		Task Run();
	}

	public interface IAsyncPulse
	{
		IAsyncPulsed SetAction(Func<object, Status> action);
	}

	public interface IAsyncPulsed
	{
		IAsyncPulsed SetInterval(TimeSpan interval);

		Task Run();
	}
}
