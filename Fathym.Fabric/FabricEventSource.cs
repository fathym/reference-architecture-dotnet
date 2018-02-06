using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Fabric
{
	[EventSource(Name = "Fifty280-Fabric")]
	public sealed class FabricEventSource : EventSource
	{
		#region Constants
		private const int MessageEventId = 1;

		private const int ServiceMessageEventId = 2;

		private const int ServiceTypeRegisteredEventId = 3;

		private const int ServiceHostInitializationFailedEventId = 4;

		private const int ServiceRequestStartEventId = 5;

		private const int ServiceRequestStopEventId = 6;

		private const int ServiceRequestFailedEventId = 7;

		private const int CycleStartEventId = 8;

		private const int CycleStopEventId = 9;

		private const int ConfigurationErrorEventId = 10;

		private const int ExceptionEventId = 11;
		#endregion

		#region Fields
		private static readonly Lazy<FabricEventSource> Instance = new Lazy<FabricEventSource>(() => new FabricEventSource());
		#endregion

		#region Properties
		public static FabricEventSource Current { get { return Instance.Value; } }
		#endregion

		#region Constructors
		static FabricEventSource()
		{
			// A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
			// This problem will be fixed in .NET Framework 4.6.2.
			Task.Run(() => { }).Wait();
		}

		private FabricEventSource() : base() { }
		#endregion

		#region Events
		#region Messaging Events
		[Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
		public void Message(string message)
		{
			if (IsEnabled())
			{
				WriteEvent(MessageEventId, message);
			}
		}

		[NonEvent]
		public void ServiceMessage(StatelessService service, string message)
		{
			ServiceMessage(service.Context, message);
		}

		[NonEvent]
		public void ServiceMessage(StatefulService service, string message)
		{
			ServiceMessage(service.Context, message);
		}

		[NonEvent]
		public void ServiceMessage(Actor service, string message)
		{
			ServiceMessage(service.ActorService.Context, message);
		}

		[NonEvent]
		public void ServiceMessage(StatelessServiceContext svcParams, string message)
		{
			if (this.IsEnabled())
			{
				ServiceMessage(
					svcParams.ServiceName.ToString(),
					svcParams.ServiceTypeName,
					svcParams.InstanceId,
					svcParams.PartitionId,
					svcParams.CodePackageActivationContext.ApplicationName,
					svcParams.CodePackageActivationContext.ApplicationTypeName,
					FabricRuntime.GetNodeContext().NodeName,
					message);
			}
		}

		[NonEvent]
		public void ServiceMessage(StatefulServiceContext svcParams, string message)
		{
			if (this.IsEnabled())
			{
				ServiceMessage(
					svcParams.ServiceName.ToString(),
					svcParams.ServiceTypeName,
					svcParams.ReplicaId,
					svcParams.PartitionId,
					svcParams.CodePackageActivationContext.ApplicationName,
					svcParams.CodePackageActivationContext.ApplicationTypeName,
					FabricRuntime.GetNodeContext().NodeName,
					message);
			}
		}

		[NonEvent]
		public void ServiceMessage(ServiceContext svcParams, string message)
		{
			if (this.IsEnabled())
			{
				ServiceMessage(
					svcParams.ServiceName.ToString(),
					svcParams.ServiceTypeName,
					0,
					svcParams.PartitionId,
					svcParams.CodePackageActivationContext.ApplicationName,
					svcParams.CodePackageActivationContext.ApplicationTypeName,
					FabricRuntime.GetNodeContext().NodeName,
					message);
			}
		}

		[Event(ServiceMessageEventId, Level = EventLevel.Informational, Message = "{7}")]
		public void ServiceMessage(string serviceName, string serviceTypeName, long replicaOrInstanceId,
			Guid partitionId, string applicationName, string applicationTypeName, string nodeName, string message)
		{
			WriteEvent(ServiceMessageEventId, serviceName, serviceTypeName, replicaOrInstanceId, partitionId,
				applicationName, applicationTypeName, nodeName, message);
		}
		#endregion

		#region Fabric Events
		[Event(ConfigurationErrorEventId, Level = EventLevel.Error, Message = "Service host process {0} registered service type {1}", Keywords = Keywords.ServiceInitialization)]
		public void ConfigurationError(string section, string config, string message)
		{
			WriteEvent(ConfigurationErrorEventId, section, config, message);
		}

		[Event(ExceptionEventId, Level = EventLevel.Error, Message = "Exception has been thrown {0}", Keywords = Keywords.ServiceInitialization)]
		public void Exception(string exception)
		{
			WriteEvent(ExceptionEventId, exception);
		}

		[Event(ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "Service host process {0} registered service type {1}", Keywords = Keywords.ServiceInitialization)]
		public void ServiceTypeRegistered(int hostProcessId, string serviceType)
		{
			WriteEvent(ServiceTypeRegisteredEventId, hostProcessId, serviceType);
		}

		[Event(ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Service host initialization failed {0}", Keywords = Keywords.ServiceInitialization)]
		public void ServiceHostInitializationFailed(string exception)
		{
			WriteEvent(ServiceHostInitializationFailedEventId, exception);
		}

		[Event(ServiceRequestStartEventId, Level = EventLevel.Informational, Message = "Service request '{0}' started", Keywords = Keywords.Requests)]
		public void ServiceRequestStart(string requestTypeName)
		{
			WriteEvent(ServiceRequestStartEventId, requestTypeName);
		}

		[Event(ServiceRequestStopEventId, Level = EventLevel.Informational, Message = "Service request '{0}' finished", Keywords = Keywords.Requests)]
		public void ServiceRequestStop(string requestTypeName)
		{
			WriteEvent(ServiceRequestStopEventId, requestTypeName);
		}

		[Event(ServiceRequestFailedEventId, Level = EventLevel.Error, Message = "Service request '{0}' failed", Keywords = Keywords.Requests)]
		public void ServiceRequestFailed(string requestTypeName, string exception)
		{
			WriteEvent(ServiceRequestFailedEventId, requestTypeName, exception);
		}

		[Event(CycleStartEventId, Level = EventLevel.Verbose, Message = "Cycle started for '{0}'", Keywords = Keywords.Cycle)]
		public void CycleStart(string serivce)
		{
			WriteEvent(CycleStartEventId, serivce);
		}

		[Event(CycleStopEventId, Level = EventLevel.Verbose, Message = "Cycle ended for '{0}', sleeping for {1}", Keywords = Keywords.Cycle)]
		public void CycleStop(string serivce, long delay)
		{
			WriteEvent(CycleStopEventId, serivce, delay);
		}
		#endregion
		#endregion

		#region Hepers
		#endregion

		#region Support
		public static class Keywords
		{
			public const EventKeywords Requests = (EventKeywords)0x1L;
			public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
			public const EventKeywords Cycle = (EventKeywords)0x4L;
		}
		#endregion
	}
}
