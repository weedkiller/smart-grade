using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FirestormSW.SmartGrade.TagHelpers
{
    [HtmlTargetElement("img")]
    public class SvgTagHelper : TagHelper
    {
        public string Src { get; set; }

        private IUrlHelperFactory urlHelperFactory;
        private IActionContextAccessor actionContextAccessor;

        public SvgTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccessor = actionContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            if (context.AllAttributes.FirstOrDefault(a => a.Name == "src")?.Value?.ToString()?.EndsWith(".svg") != true ||
                context.AllAttributes.FirstOrDefault(a => a.Name == "data-svg")?.Value?.ToString()?.ToLower() != "true")
                output.Attributes.Add("src", urlHelper.Content(Src));
            else
            {
                var path = "wwwroot" + urlHelper.Content(Src);
                if (File.Exists(path))
                {
                    output.TagMode = TagMode.StartTagAndEndTag;
                    output.SuppressOutput();
                    output.Content.SetHtmlContent(File.ReadAllText(path));
                }
            }
        }
    }
}