using System.IO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class HtmlHelperEx
    {
        public static IHtmlContent Resource(this IHtmlHelper<dynamic> helper, string path)
        {
            path = Startup.HostEnvironment.WebRootPath + path.TrimStart('~');
            if (File.Exists(path))
                return new HtmlString(File.ReadAllText(path));

            return new HtmlString("");
        }
    }
}