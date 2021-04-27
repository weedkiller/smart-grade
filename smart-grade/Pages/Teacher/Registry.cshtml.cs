using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Teacher
{
    [Authorize(Policy = UserClaims.Teacher)]
    public class Registry : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}