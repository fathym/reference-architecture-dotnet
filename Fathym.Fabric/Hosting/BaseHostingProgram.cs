using Fathym.Fabric.Runtime;
using Fathym.Fabric.Runtime.Host;
using Fathym.Fluent;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Text;
using System.Threading;

namespace Fathym.Fabric.Hosting
{
	public class BaseHostingProgram<TProgram>
		where TProgram : class, new()
	{
		#region Properties
		public static TProgram Host = new TProgram();
		#endregion

		#region API methods
		public virtual IHostingPipeline EstablishHostingPipeline()
		{
			return new FathymHostingPipeline();
		}
		#endregion
	}

	public interface IHostingPipeline
	{
		IHostingPipeline Host(Func<string> action);

		IHostingPipeline Log(string name);

		void Run();
	}

	public class FathymHostingPipeline : BaseOrderedPipeline, IHostingPipeline
	{
		#region Helpers
		protected string loggingName;

		protected List<Func<string>> hostActions;
		#endregion

		#region Constructors
		public FathymHostingPipeline()
			: base()
		{
			hostActions = new List<Func<string>>();
		}
		#endregion

		#region API Methods
		public virtual IHostingPipeline Log(string name)
		{
			loggingName = name;

			return this;
		}

		public virtual IHostingPipeline Host(Func<string> action)
		{
			if (action != null)
			{
				hostActions.Add(action);

				addAction(runHost);
			}

			return this;
		}
		#endregion

		#region Helpers
		protected virtual void executeHostActions()
		{
			hostActions.Each(ha =>
			{
				try
				{
					var serviceType = ha();

					FabricEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceType);
				}
				catch (Exception e)
				{
					FabricEventSource.Current.ServiceHostInitializationFailed(e.ToString());

					throw;
				}
			});

			Thread.Sleep(Timeout.Infinite);
		}

		protected virtual void runHost()
		{
			try
			{
				if (loggingName.IsNullOrEmpty())
				{
					executeHostActions();
				}
				else
				{
					using (var diagnosticsPipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline(loggingName))
					{
						executeHostActions();
					}
				}
			}
			catch (Exception e)
			{
				FabricEventSource.Current.Exception(e.ToString());

				throw;
			}
		}
		#endregion
	}
}
