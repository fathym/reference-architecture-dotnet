using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Fluent
{
    public class BaseOrderedPipeline
	{
		#region Fields
		protected List<Action> actions;
		#endregion

		#region Constructors
		public BaseOrderedPipeline()
		{
			actions = new List<Action>();
		}
		#endregion

		#region API Methods
		public virtual void Run()
		{
			runActions();
		} 
		#endregion

		#region Helpers
		protected virtual void addAction(Action action)
		{
			actions.Add(action);
		}

		protected virtual void runActions()
		{
			actions.Each(action => action());

			actions.Clear();
		} 
		#endregion

	}
}
