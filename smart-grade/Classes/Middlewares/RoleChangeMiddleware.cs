using System;
using System.Linq;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Middlewares
{
    public class RoleChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LoginService loginService, LanguageService languageService, AppDatabase database)
        {
            var newRoleCode = context.Request.Query["role"].ToString();
            if (!Enum.TryParse<GroupType>(newRoleCode, out var newRole))
            {
                await _next(context);
                return;
            }

            var currentUser = loginService.GetCurrentLoggedInUser(context);
            var newRoleGroup = currentUser?.Groups?.SingleOrDefault(g => g.GroupType == newRole);

            if (newRoleGroup != null)
            {
                currentUser.CurrentRole = newRoleGroup;
                database.SaveChanges();

                var url = new Uri($"{context.Request.Scheme}://{context.Request.Host}{loginService.GetStartingPage(currentUser)}{context.Request.QueryString}");
                context.Response.Redirect(url.RemoveQueryStringByKey("role"));
            }
            else
                await _next(context);
        }
    }
}