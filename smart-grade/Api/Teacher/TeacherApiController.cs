using System;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FirestormSW.SmartGrade.Api.Teacher
{
    [ApiController]
    [Route("api/teacher")]
    [Authorize(Policy = UserClaims.Teacher)]
    public class TeacherApiController : ControllerBase
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;
        private readonly IServiceProvider serviceProvider;

        public TeacherApiController(AppDatabase database, LoginService loginService, IServiceProvider serviceProvider)
        {
            this.database = database;
            this.loginService = loginService;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("users_in_class")]
        public ActionResult GetClass(int classId, int subjectId, string summaryProviderTypeName)
        {
            var summaryProviderType = typeof(Program).Assembly.GetTypes().FirstOrDefault(t => t.Name == summaryProviderTypeName);
            ISummaryProvider summaryProvider = null;
            if (summaryProviderType != null)
                summaryProvider = ActivatorUtilities.CreateInstance(serviceProvider, summaryProviderType) as ISummaryProvider;

            var currentUser = loginService.GetCurrentLoggedInUser(HttpContext);

            var group = database.Groups
                .Include(g => g.Users)
                .Include(g => g.FormMaster)
                .GetById(classId);
            var users = group.Users
                .OrderBy(u => u.FullName)
                .AsEnumerable()
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    LoginName = u.LoginName,
                    PlatformId = u.PlatformId,
                    Summary = summaryProvider?.GetSummary(u, database.Subjects.GetById(subjectId))
                });
            
            // Update history
            var historyEntries = database.TeacherClassHistory
                .Include(h => h.Teacher)
                .Include(h => h.Class)
                .Where(h => h.Teacher.Id == currentUser.Id && h.Class.Id == classId)
                .ToList();
            database.TeacherClassHistory.RemoveRange(historyEntries);
            database.TeacherClassHistory.Add(new TeacherClassHistory
            {
                Class = database.Groups.GetById(classId),
                Teacher = currentUser
            });
            database.SaveChanges();

            return Ok(new
            {
                Users = users,
                IsFormMaster = group.FormMaster?.Id == currentUser.Id,
                FirstAvailableSubjectId = currentUser.TaughtSubjects.FirstOrDefault()?.Id ?? -1
            });
        }
    }
}