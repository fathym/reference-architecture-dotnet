using System;
using System.Collections.Generic;
using System.Text;

namespace Fathym.Presentation.Fluent
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

		#region Helpers
		protected virtual void addAction(Action action)
		{
			actions.Add(action);
		}

		protected virtual void runActions()
		{
			actions.ForEach(action => action());

			actions.Clear();
		} 
		#endregion

	}
}
