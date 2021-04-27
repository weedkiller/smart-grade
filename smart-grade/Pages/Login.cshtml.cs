using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Resources.Localization;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FirestormSW.SmartGrade.Pages
{
    [AllowAnonymous]
    public class Login : PageModel
    {
        public string LoginError { get; private set; }

        private readonly IStringLocalizer<Localization> localizer;
        private readonly LoginService loginService;
        private readonly AppDatabase database;

        public Login(IStringLocalizer<Localization> localizer, LoginService loginService, AppDatabase database)
        {
            this.localizer = localizer;
            this.loginService = loginService;
            this.database = database;
        }

        public void OnGet()
        {
            if (int.TryParse(Request.Query["Impersonate"], out var userId))
                loginService.ImpersonateUser(HttpContext, database.Users.Include(u => u.Groups).GetById(userId));
            else if(loginService.GetCurrentLoggedInUser(HttpContext) is var loggedInUser && loggedInUser != null)
                Response.Redirect(loginService.GetStartingPage(loggedInUser));
        }

        public async void OnPost()
        {
            HttpContext.Session.Clear();

            var authenticatedUser = loginService.GetCurrentLoggedInUser(HttpContext);
            if (authenticatedUser != null)
                Response.Redirect(loginService.GetStartingPage(authenticatedUser));

            var username = Request.Form["username"];
            var password = Request.Form["password"];

            var result = await loginService.SignInWithCredentials(HttpContext, username, password);
            if (!result)
                LoginError = "Unable to log in";
        }
    }
}