using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Fathym.Fabric.Runtime.Adapters;
using Fathym.Presentation.MVC.Fluent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Fathym.Presentation.MVC
{
	public class FathymAPIStartup : FathymStartup
	{
		#region Constructors
		public FathymAPIStartup(IHostingEnvironment env, ILoggerFactory loggerFactory, IFabricAdapter fabricAdapter)
			: base(env, loggerFactory, fabricAdapter)
		{ }
		#endregion

		#region Helpers
		protected override void setupCoreBuilder(ICoreBuilderPipeline pipeline)
		{
			pipeline
				.Logging("Logging")
				.ExceptionHandling(null)
				.Build();
		}

		//protected override void setupCoreServices(ICoreServicesPipeline pipeline)
		//{
		//	var dpConnConfig = loadDataProtectionConnectionConfig();

		//	var dpContConfig = loadDataProtectionContainerConfig();

		//	var dpBlobConfig = loadDataProtectionBlobConfig();

		//	pipeline
		//		.Config()
		//		.Caching()
		//		.AppInsights()
		//		//.Compression()
		//		.DataProtection(dpConnConfig, dpContConfig, dpBlobConfig)
		//		.Sessions(configureSessionOptions)
		//		.Set();
		//}

		protected override void setupProxyBuilder(IProxyBuilderPipeline pipeline)
		{ }

		protected override void setupViewBuilder(IViewBuilderPipeline pipeline)
		{
			pipeline
				.MVC()
				.Build();
		}

		protected override void setupViewServices(IViewServicesPipeline pipeline)
		{
			pipeline
				.MVC(assemblies: new List<Assembly>()
				{
					Assembly.GetEntryAssembly()
				})
				.Set();
		}
		#endregion
	}
}
