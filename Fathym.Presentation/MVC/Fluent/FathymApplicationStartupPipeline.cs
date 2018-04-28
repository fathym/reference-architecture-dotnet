using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fathym.Presentation.MVC.Fluent
{
	public class FathymApplicationStartupPipeline : IApplicationStartupPipeline
	{
		public virtual IConfigurationBuilderPipeline Configure()
		{
			return new FathymConfigurationBuilderPipeline();
		}
	}

	public interface IApplicationStartupPipeline
	{
		IConfigurationBuilderPipeline Configure();
	}

	#region Configuration
	public class FathymConfigurationBuilderPipeline : IConfigurationBuilderPipeline
	{
		#region Fields
		protected readonly Dictionary<string, bool> configFiles;

		protected readonly List<string> configGlobs;
		#endregion

		#region Constructors
		public FathymConfigurationBuilderPipeline()
		{
			configFiles = new Dictionary<string, bool>();

			configGlobs = new List<string>();
		}
		#endregion

		#region API Methods
		public virtual IConfigurationBuilderPipeline AddConfig(string filePath, bool reloadOnChange = false)
		{
			if (!configFiles.ContainsKey(filePath))
				configFiles.Add(filePath, reloadOnChange);

			return this;
		}

		public virtual IConfigurationBuilderPipeline AddConfigGlob(string globPattern)
		{
			if (!configGlobs.Contains(globPattern))
				configGlobs.Add(globPattern);

			return this;
		}

		public virtual IConfigurationRoot Build(IHostingEnvironment env)
		{
			return buildConfigurationRoot(env);
		}
		#endregion

		#region Helpers
		protected virtual void addJsonConfigs(IConfigurationBuilder builder, IHostingEnvironment env)
		{
			var configs = new Matcher();

			configGlobs.ForEach(pattern => configs.AddInclude(pattern));

			var results = configs.Execute(new DirectoryInfoWrapper(new DirectoryInfo(env.ContentRootPath)));

			if (!results.Files.IsNullOrEmpty())
				results.Files.ForEach(file => builder.AddJsonFile(file.Path));
		}

		protected virtual IConfigurationRoot buildConfigurationRoot(IHostingEnvironment env)
		{
			var builder = loadConfigurationBuilder(env);

			return builder.Build();
		}

		protected virtual IConfigurationBuilder loadConfigurationBuilder(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath);

			configFiles.ForEach(file => builder.AddJsonFile(file.Key, optional: true, reloadOnChange: file.Value));

			addJsonConfigs(builder, env);

			builder.AddEnvironmentVariables();

			return builder;
		}
		#endregion
	}

	public interface IConfigurationBuilderPipeline
	{
		IConfigurationBuilderPipeline AddConfigGlob(string globPattern);

		IConfigurationBuilderPipeline AddConfig(string filePath, bool reloadOnChange = false);

		IConfigurationRoot Build(IHostingEnvironment env);
	}
	#endregion
}
