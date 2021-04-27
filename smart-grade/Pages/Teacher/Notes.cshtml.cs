using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Teacher
{
    [Authorize(Policy = UserClaims.Teacher)]
    public class Notes : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}