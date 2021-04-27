using System;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Middlewares
{
    public class AuthCookieTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthCookieTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Cookies.Append("AuthTimeout", (DateTime.UtcNow + LoginService.SessionTimeout).ToString("R"),
                new CookieOptions {HttpOnly = false});
            await _next(context);
        }
    }
}