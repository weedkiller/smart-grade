using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Admin
{
    [Authorize(Policy = UserClaims.Administrator)]
    public class Teachers : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}