using System;
using System.Linq;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Middlewares
{
    public class LanguageChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LoginService loginService, LanguageService languageService, AppDatabase database)
        {
            var newLangCode = context.Request.Query["lang"].ToString();
            var newCulture = languageService
                .GetAvailableCultures()
                .FirstOrDefault(c => string.Equals(c.Name, newLangCode, StringComparison.InvariantCultureIgnoreCase));
            var currentUser = loginService.GetCurrentLoggedInUser(context);
            
            if (currentUser != null && newCulture != null)
            {
                currentUser.PreferredLanguage = newCulture.Name;
                database.SaveChanges();

                var url = new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
                context.Response.Redirect(url.RemoveQueryStringByKey("lang"));
            }
            else
                await _next(context);
        }
    }
}