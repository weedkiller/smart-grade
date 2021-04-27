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
    public class SubjectChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public SubjectChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LoginService loginService, AppDatabase database)
        {
            if (!int.TryParse(context.Request.Query["subj"].ToString(), out var newSubjectId))
            {
                await _next(context);
                return;
            }

            context.Session.SetInt32("subject_id", newSubjectId);
            var url = new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
            context.Response.Redirect(url.RemoveQueryStringByKey("subj"));
        }
    }
}