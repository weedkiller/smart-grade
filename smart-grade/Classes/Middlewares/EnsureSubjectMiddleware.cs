using System.Linq;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Middlewares
{
    public class EnsureSubjectMiddleware
    {
        private readonly RequestDelegate _next;

        public EnsureSubjectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LoginService loginService)
        {
            int subjectId = context.Session.GetInt32("subject_id") ?? -1;
            if (subjectId == -1)
            {
                var currentUser = loginService.GetCurrentLoggedInUser(context);
                var subject = currentUser?.TaughtSubjects.FirstOrDefault();
                if (subject != null)
                    context.Session.SetInt32("subject_id", subject.Id);
            }

            await _next(context);
        }
    }
}