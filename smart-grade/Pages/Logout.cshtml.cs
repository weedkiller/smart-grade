using System.Linq;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages
{
    public class Logout : PageModel
    {
        private readonly LoginService loginService;

        public Logout(LoginService loginService)
        {
            this.loginService = loginService;
        }

        public async void OnGet()
        {
            if (Request.Query["StopImpersonating"].Any())
            {
                loginService.StopImpersonatingUser(HttpContext);
                return;
            }
            
            HttpContext.Session.Clear();
            await loginService.SignOut(HttpContext);
            Response.Redirect("/Login");
        }
    }
}