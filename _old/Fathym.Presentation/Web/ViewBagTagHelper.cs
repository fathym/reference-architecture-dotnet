using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.Web
{
	[HtmlTargetElement("fathym", Attributes = viewBagAttributeName)]
	public class ViewBagTagHelper : TagHelper
	{
		#region Fields
		protected const string viewBagAttributeName = "view-bag";

		protected const string viewBagPropertyAttributeName = "view-bag-prop";
		#endregion

		#region Properties
		[HtmlAttributeName(viewBagAttributeName)]
		public object ViewBag { get; set; }

		[HtmlAttributeName(viewBagPropertyAttributeName)]
		public string ViewBagProperty { get; set; }
		#endregion

		#region Constructors
		public ViewBagTagHelper()
		{
			ViewBagProperty = "ViewBag";
		}
		#endregion

		#region API methods
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "script";

			output.TagMode = TagMode.StartTagAndEndTag;

			output.Content.SetHtmlContent($"window.{ViewBagProperty} = {ViewBag.ToJSON()};");
		}
		#endregion
	}
}
