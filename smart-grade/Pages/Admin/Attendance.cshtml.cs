using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Admin
{
    [Authorize(Policy = UserClaims.Administrator)]
    public class Attendance : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}