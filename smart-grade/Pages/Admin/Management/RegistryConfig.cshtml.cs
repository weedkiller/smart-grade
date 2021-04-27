using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Admin.Management
{
    [Authorize(Policy = UserClaims.Administrator)]
    public class RegistryConfig : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}