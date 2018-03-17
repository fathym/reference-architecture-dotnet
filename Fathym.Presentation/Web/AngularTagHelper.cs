using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Web
{
	[HtmlTargetElement("angular", Attributes = startupAttributeName)]
	public class AngularTagHelper : TagHelper
	{
		#region Fields
		protected readonly IHostingEnvironment env; 

		protected const string startupAttributeName = "startup";

		protected const string systemJsAttributeName = "systemJs";
		#endregion

		#region Properties
		[HtmlAttributeName(startupAttributeName)]
		public virtual string Application { get; set; }

		[HtmlAttributeName(systemJsAttributeName)]
		public virtual bool SystemJS { get; set; }
		#endregion

		#region Constructors
		public AngularTagHelper(IHostingEnvironment env)
		{
			this.env = env;
		}
		#endregion

		#region API methods
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "script";

			output.TagMode = TagMode.StartTagAndEndTag;

			output.PostElement.AppendHtml("<script>window.module = 'fifty280';</script>");

			var version = (Assembly.GetEntryAssembly() ?? GetType().Assembly)
						.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
						.InformationalVersion;

			if (SystemJS)
			{
				output.Attributes.SetAttribute("src", "/js/lib/core-js/shim.min.js");

				output.PostElement.AppendHtml($"<script src='/js/lib/zone/zone.js'></script>");

				output.PostElement.AppendHtml($"<script src='/js/lib/reflect/Reflect.js'></script>");

				if (env.IsEnvironment("Development"))
				{
					output.PostElement.AppendHtml($"<script src='/js/lib/sysjs/system.src.js'></script>");

					output.PostElement.AppendHtml($"<script src='/js/{Application}/systemjs.config.js?v={version}'></script>");

					output.PostElement.AppendHtml($"<script>System.import('{Application}').catch(function (err) {{ console.error(err); }});</script>");
				}
				else
					output.PostElement.AppendHtml($"<script src='/js/{Application}.bundle.js.gz?v={version}'></script>");
			}
			else if (!env.WebRootPath.IsNullOrEmpty())
			{
				var rootDir = new DirectoryInfo(env.WebRootPath);

				var appDir = rootDir.GetDirectories(Application).FirstOrDefault();

				if (appDir != null)
				{
					var inline = appDir.GetFiles("inline.bundle.js").FirstOrDefault();

					if (inline != null && inline.Exists)
					{
						var scripts = appDir.GetFiles("scripts.bundle.js").FirstOrDefault();

						output.Attributes.SetAttribute("src", $"/{Application}/inline.bundle.js?v={version}");

						output.PostElement.AppendHtml($"<script src='/{Application}/polyfills.bundle.js?v={version}'></script>");

						output.PostElement.AppendHtml($"<script src='/{Application}/styles.bundle.js?v={version}'></script>");

						if (scripts != null && scripts.Exists)
							output.PostElement.AppendHtml($"<script src='/{Application}/scripts.bundle.js?v={version}'></script>");

						output.PostElement.AppendHtml($"<script src='/{Application}/vendor.bundle.js?v={version}'></script>");

						output.PostElement.AppendHtml($"<script src='/{Application}/main.bundle.js?v={version}'></script>");
					}
					else
					{
						inline = appDir.GetFiles("inline.*.bundle.js").FirstOrDefault();

						var polyfills = appDir.GetFiles("polyfills.*.bundle.js").FirstOrDefault();

						var styles = appDir.GetFiles("styles.*.bundle.css").FirstOrDefault();

						var scripts = appDir.GetFiles("scripts.*.bundle.js").FirstOrDefault();

						var vendor = appDir.GetFiles("vendor.*.bundle.js").FirstOrDefault();

						var main = appDir.GetFiles("main.*.bundle.js").FirstOrDefault();

						if (inline != null && inline.Exists)
							output.Attributes.SetAttribute("src", $"{resolveRelativePath(rootDir, inline)}?v={version}");

						if (polyfills != null && polyfills.Exists)
							output.PostElement.AppendHtml($"<script src='{resolveRelativePath(rootDir, polyfills)}?v={version}'></script>");

						if (styles != null && styles.Exists)
							output.PostElement.AppendHtml($"<link href='{resolveRelativePath(rootDir, styles)}?v={version}' rel='stylesheet' />");

						if (scripts != null && scripts.Exists)
							output.PostElement.AppendHtml($"<script src='{resolveRelativePath(rootDir, scripts)}?v={version}'></script>");

						if (vendor != null && vendor.Exists)
							output.PostElement.AppendHtml($"<script src='{resolveRelativePath(rootDir, vendor)}?v={version}'></script>");

						if (main != null && main.Exists)
							output.PostElement.AppendHtml($"<script src='{resolveRelativePath(rootDir, main)}?v={version}'></script>");
					}
				}
				else
					output.PostElement.AppendHtml($"<span>Unable to locate application bundles for {Application}");
			}
			else
				output.PostElement.AppendHtml($"<span>The wwwroot folder was not added on deploy for {Application}.  Update the Web project file to Copy wwwroot files on publish.");
		}
		#endregion

		#region Helpers
		protected virtual string resolveRelativePath(DirectoryInfo rootDir, FileInfo file)
		{
			return file.FullName.Replace(rootDir.FullName, String.Empty)
				.Replace(@"\", "/");
		}
		#endregion
	}
}
