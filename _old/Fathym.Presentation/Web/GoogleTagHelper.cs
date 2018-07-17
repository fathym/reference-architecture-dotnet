using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Web
{
	[HtmlTargetElement("google", Attributes = googleAnalyticsAttributeName)]
	public class GoogleTagHelper : TagHelper
	{
		#region Fields
		protected readonly IHostingEnvironment env;

		protected const string debugAttributeName = "debug";

		protected const string googleAnalyticsAttributeName = "ga";

		protected const string googleAnalyticsExperimentAttributeName = "ga-experiment";

		protected const string googleFontsAttributeName = "fonts";

		protected const string googleIconsAttributeName = "icons";

		protected const string googleMapsAttributeName = "maps";
		#endregion

		#region Properties
		[HtmlAttributeName(googleAnalyticsAttributeName)]
		public virtual string Code { get; set; }

		[HtmlAttributeName(debugAttributeName)]
		public virtual bool Debug { get; set; }

		[HtmlAttributeName(googleAnalyticsExperimentAttributeName)]
		public virtual string Experiment { get; set; }

		[HtmlAttributeName(googleFontsAttributeName)]
		public virtual Dictionary<string, string> Fonts { get; set; }

		[HtmlAttributeName(googleIconsAttributeName)]
		public virtual List<string> Icons { get; set; }

		[HtmlAttributeName(googleMapsAttributeName)]
		public string MapKey { get; set; }
		#endregion

		#region Constructors
		public GoogleTagHelper(IHostingEnvironment env)
		{
			this.env = env;
		}
		#endregion

		#region API methods
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "script";

			output.TagMode = TagMode.StartTagAndEndTag;

			var sb = new StringBuilder();

			sb.AppendLine("window.ga = window.ga || function () { (ga.q = ga.q || []).push(arguments) }; ga.l = +new Date;");

			if (Debug)
				sb.AppendLine("window.ga_debug = {trace: true};");

			sb.AppendLine($"ga('create', '{Code}', 'auto');");

			output.Content.SetHtmlContent(sb.ToString());

			var debugPart = Debug ? "_debug" : "";

			output.PostElement.AppendHtml(scriptWrite($"//www.google-analytics.com/analytics{debugPart}.js"));

			if (!Experiment.IsNullOrEmpty())
				output.PostElement.AppendHtml(scriptWrite($"//www.google-analytics.com/cx/api.js?experiment={Experiment}"));

			if (!MapKey.IsNullOrEmpty())
				output.PostElement.AppendHtml($"<script src='https://maps.googleapis.com/maps/api/js?key={MapKey}'></script>");

			Icons?.ForEach(icon => output.PostElement.AppendHtml($"<link href='https://fonts.googleapis.com/icon?family={icon}' rel='stylesheet'>"));

			Fonts?.Each(font => output.PostElement.AppendHtml($"<link href='https://fonts.googleapis.com/css?family={font.Key}:{font.Value}' rel='stylesheet' type='text/css'>"));
		}
		#endregion

		#region Helpers
		protected virtual string resolveRelativePath(DirectoryInfo rootDir, FileInfo file)
		{
			return file.FullName.Replace(rootDir.FullName, String.Empty)
				.Replace(@"\", "/");
		}

		protected virtual string scriptWrite(string srcPath)
		{
			return $"<script src='{srcPath}'></script>";
		}
		#endregion
	}
}
