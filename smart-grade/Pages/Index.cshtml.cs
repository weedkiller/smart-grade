using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages
{
    public class Index : PageModel
    {
        private readonly LoginService loginService;

        public Index(LoginService loginService)
        {
            this.loginService = loginService;
        }

        public ActionResult OnGet()
        {
            var currentUser = loginService.GetCurrentLoggedInUser(HttpContext);
            if (currentUser != null)
                return Redirect(loginService.GetStartingPage(currentUser));

            return Forbid();
        }

        public ActionResult OnPost() => new NoContentResult();
    }
}