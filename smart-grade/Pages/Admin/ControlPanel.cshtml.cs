using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Admin
{
    [Authorize(Policy = UserClaims.Administrator)]
    public class ControlPanel : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}