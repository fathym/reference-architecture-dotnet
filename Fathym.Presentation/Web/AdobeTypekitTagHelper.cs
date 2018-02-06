using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Web
{
	[HtmlTargetElement("adobe", Attributes = typekitAttributeName)]
	public class AdobeTypekitTagHelper : TagHelper
	{
		#region Fields
		protected const string typekitAttributeName = "typekit";
		#endregion

		#region Properties
		[HtmlAttributeName(typekitAttributeName)]
		public virtual string Typekit { get; set; }
		#endregion

		#region Constructors
		public AdobeTypekitTagHelper()
		{ }
		#endregion

		#region API methods
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Typekit.IsNullOrEmpty())
			{
				output.SuppressOutput();

				return;
			}

			var isFirst = output.TagName == "adobe";

			var script = new StringBuilder();

			script.AppendLine("	(function(d) {");

			script.AppendLine("		var config = {");

			script.AppendLine($"		kitId: '{Typekit}',");

			script.AppendLine("			scriptTimeout: 3000,");

			script.AppendLine("			async: true");

			script.AppendLine("		},");

			script.AppendLine("		h =d.documentElement,t=setTimeout(function(){h.className=h.className.replace(/\bwf-loading\b/g,'')+' wf-inactive';},config.scriptTimeout),tk=d.createElement('script'),f=false,s=d.getElementsByTagName('script')[0],a;h.className+=' wf-loading';tk.src='https://use.typekit.net/'+config.kitId+'.js';tk.async=true;tk.onload=tk.onreadystatechange=function(){a=this.readyState;if(f||a&&a!='complete'&&a!='loaded')return;f=true;clearTimeout(t);try{Typekit.load(config)}catch(e){}};s.parentNode.insertBefore(tk,s)");

			script.AppendLine("	})(document);");

			if (isFirst)
			{
				output.TagName = "script";

				output.TagMode = TagMode.StartTagAndEndTag;

				output.Content.SetHtmlContent(script.ToString());
			}
			else
			{
				output.PostElement.AppendHtml($"<script>{script.ToString()}</script>");
			}
		}
		#endregion
	}
}
