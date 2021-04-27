using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Student
{
    [Authorize(Policy = UserClaims.Student)]
    public class Disciplinary : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}