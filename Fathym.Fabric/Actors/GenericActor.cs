using Fathym.Design;
using Fathym.Fabric.Runtime.Adapters;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Threading.Tasks;

namespace Fathym.Fabric.Actors
{
	public abstract class GenericActor : Actor, IRemindable
	{
		#region Fields
		protected readonly IFabricAdapter fabricAdapter;

		protected IActorReminder refreshReminder;
		#endregion

		#region Properties
		public virtual bool EnableRefresh { get; set; }

		public virtual string RefreshReminderName { get; set; }
		#endregion

		#region Constructors
		public GenericActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{
			DesignOutline.Instance.SetupCommonDefaultJSONSerialization();

			fabricAdapter = new StatefulFabricAdapter(actorService.Context);

			RefreshReminderName = "Refresh";
		}
		#endregion

		#region Runtime
		protected override async Task OnActivateAsync()
		{
			setupLogging();

			if (EnableRefresh)
				await setupActorRefresh();

			FabricEventSource.Current.ServiceMessage(this, $"Activated {ActorService.Context.ServiceName}");

			await base.OnActivateAsync();
		}

		protected override async Task OnDeactivateAsync()
		{
			FabricEventSource.Current.ServiceMessage(this, $"Deactivated {ActorService.Context.ServiceName}");

			await base.OnDeactivateAsync();
		}

		public virtual async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
		{
			if (reminderName == RefreshReminderName)
				await Refresh();
		}
		#endregion

		#region API Methods
		public abstract Task Refresh();
		#endregion

		#region Helpers
		protected virtual async Task<IActorReminder> buildReminder(string name, byte[] state, TimeSpan period)
		{
			return await RegisterReminderAsync(name, state, period, period);
		}

		protected virtual T loadConfigSetting<T>(string section, string name)
		{
			return fabricAdapter.GetConfiguration().LoadConfigSetting<T>(section, name);
		}

		protected virtual T loadConfigSetting<T>(string name)
		{
			return loadConfigSetting<T>(GetType().FullName, name);
		}

		protected virtual TimeSpan loadActorRefreshRate()
		{
			return TimeSpan.FromMinutes(30);
		}

		protected virtual async Task setupActorRefresh()
		{
			var period = loadActorRefreshRate();

			try
			{
				refreshReminder = GetReminder(RefreshReminderName);
			}
			catch (ReminderNotFoundException rex)
			{
			}

			if (refreshReminder == null)
				refreshReminder = await buildReminder(RefreshReminderName, null, period);
		}

		protected virtual void setupLogging()
		{ }
		#endregion
	}
}
