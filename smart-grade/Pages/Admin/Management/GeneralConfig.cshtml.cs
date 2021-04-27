using System;
using System.Diagnostics;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FirestormSW.SmartGrade.Pages.Admin.Management
{
    [Authorize(Policy = UserClaims.Administrator)]
    public class GeneralConfig : PageModel
    {
        public long MemoryUsed { get; set; }
        public long MemoryAvailable { get; set; }
        public int MemoryPercentage => (int) (MemoryUsed * 100f / MemoryAvailable); 
        
        public void OnGet()
        {
            using (var process = Process.GetCurrentProcess())
                MemoryUsed = process.PrivateMemorySize64;
            MemoryAvailable = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        }
    }
}