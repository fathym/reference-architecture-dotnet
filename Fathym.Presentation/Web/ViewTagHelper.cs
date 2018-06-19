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
	[HtmlTargetElement("view", Attributes = startupAttributeName)]
	public class ViewTagHelper : TagHelper
	{
		#region Fields
		protected readonly IHostingEnvironment env;

		protected const string startupAttributeName = "startup";
		#endregion

		#region Properties
		[HtmlAttributeName(startupAttributeName)]
		public virtual Dictionary<string, string> ApplicationFiles { get; set; }
		#endregion

		#region Constructors
		public ViewTagHelper(IHostingEnvironment env)
		{
			this.env = env;

			ApplicationFiles = new Dictionary<string, string>();
		}
		#endregion

		#region API methods
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "script";

			output.TagMode = TagMode.StartTagAndEndTag;

			//output.PostElement.AppendHtml("<script>window.module = 'fathym';</script>");

			if (!ApplicationFiles.IsNullOrEmpty())
			{
				var jsFiles = ApplicationFiles.Where(ap => ap.Value == "JS").Select(ap => ap.Key);

				var cssFiles = ApplicationFiles.Where(ap => ap.Value == "CSS").Select(ap => ap.Key);

				var first = true;

				jsFiles.Each(
					(appFile) =>
					{
						if (first)
						{
							output.Attributes.SetAttribute("src", appFile);

							first = false;
						}
						else
							output.PostElement.AppendHtml($"<script src='{appFile}'></script>");
					});

				cssFiles.Each(
					(appFile) =>
					{
						output.PostElement.AppendHtml($"<link href='{appFile}' rel='stylesheet' />");
					});
			}
			else
			{
				output.PostElement.AppendHtml("<span>Unable to locate application files</span><script>setInterval(() => { location.href = location.href; }, 5000);</script>");
			}
		}
		#endregion

		#region Helpers
		#endregion
	}
}
