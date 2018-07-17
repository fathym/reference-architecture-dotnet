﻿using Fathym.Design.Fluent;
using Fathym.Design.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Design
{
	public class DesignOutline : DesignOutline<DesignOutline>
	{ }

	/// <summary>
	///     Contains common template methods that can be used for common functionality.
	/// </summary>
	public class DesignOutline<TOutline> : ActivatorSingleton<TOutline>
		where TOutline : class
	{
		#region Properties
		#endregion

		#region Constructors
		public DesignOutline()
		{ }
		#endregion

		#region API Methods
		public virtual IAsync Async()
		{
			return new Async();
		}

		public virtual IChain<T> Chain<T>()
		{
			return new ChainOfResponsibility<T>();
		}

		public virtual IRetry Retry()
		{
			return new Retry();
		}

		public virtual IWeightedRandom<T> WeightedRandom<T>()
		{
			return new WeightedRandom<T>();
		}

		#region Safe Lock
		public virtual void SafeLock(object lockObject, Func<bool> shouldProceedCheck, Action action)
		{
			if (shouldProceedCheck())
				lock (lockObject)
					if (shouldProceedCheck())
						action();
		}

		public virtual Task SafeLockAsync(object lockObject, Func<Task<bool>> shouldProceedCheck, Func<Task> action)
		{
			if (shouldProceedCheck().Result)
				lock (lockObject)
					if (shouldProceedCheck().Result)
						action().Wait();

			return Task.FromResult(true);
		}
		#endregion

		#region Setup Common Default JSON Serialization
		public virtual JsonSerializerSettings BuildCommonDefaultJSONSerialization(bool indent = false)
		{
			return new JsonSerializerSettings()
			{
				Converters = new[] { new StringEnumConverter() },
				Formatting = indent ? Formatting.Indented : Formatting.None
			};
		}

		public virtual void SetupCommonDefaultJSONSerialization(bool indent = true)
		{
			if (JsonConvert.DefaultSettings == null)
				JsonConvert.DefaultSettings = () => BuildCommonDefaultJSONSerialization(indent);
		}
		#endregion
		#endregion
	}
}
