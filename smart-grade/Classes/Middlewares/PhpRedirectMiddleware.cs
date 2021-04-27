using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Middlewares
{
    public class PhpRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public PhpRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().Split('?')[0].EndsWith(".php"))
                context.Response.Redirect("/Index");
            else
                await _next(context);
        }
    }
}