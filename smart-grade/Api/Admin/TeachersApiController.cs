using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Api.Responses;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/teachers")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class TeachersApiController : UserCrudController<BasicUserResponse>
    {
        protected override IQueryable<User> BaseQuery => database.Users
            .Include(u => u.Groups)
            .ThenInclude(g => g.GradeLevel)
            .Include(u => u.TeacherGradeLevel)
            .Include(u => u.TaughtClasses)
            .Include(u => u.TaughtSubjects)
            .Where(u => u.Groups.Any(g => g.GroupType == GroupType.Teacher));

        protected override Func<IEnumerable<User>, IEnumerable<BasicUserResponse>> Transform => input => input
            .Select(u => new BasicUserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                LoginName = u.LoginName,
                PlatformID = u.PlatformId,
                PreferredLanguage = u.PreferredLanguage,
                LastLogin = u.LastLogin,
                GradeType = u.TeacherGradeLevel,
                IsAdmin = u.Groups.Any(g => g.GroupType == GroupType.Admin),
                HasPassword = u.HasPassword,
                Classes = u.TaughtClasses
            });

        protected override DbSet<User> EntrySet => database.Users;
        
        protected override Func<BasicUserResponse, dynamic> ExportTransform => item => new
        {
            Name = item.FullName,
            LoginName = item.LoginName,
            PlatformID = item.PlatformID,
            GradeType = item.GradeType.Name,
            LastLogin = item.LastLogin,
        };

        public TeachersApiController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }

        protected override ActionResult UserCreatedOrUpdated(User user)
        {
            int gradeTypeId = int.Parse(Request.Form["grade_type"].ToString());
            int[] classIds = Request.Form["classes"].Select(int.Parse).ToArray();
            bool isAdmin = Request.Form["is_admin"] == "on";
            var adminGroup = database.Groups.Single(g => g.GroupType == GroupType.Admin);
            var teacherGroup = database.Groups.Single(g => g.GroupType == GroupType.Teacher);
            var gradeType = database.GradeLevels.SingleOrDefault(g => g.Id == gradeTypeId);
            var classes = database.Groups.Where(g => classIds.Contains(g.Id));

            if (gradeType == null)
            {
                return BadRequest(new
                {
                    Error = true,
                    Message = "The Grade Type must be set for this teachers."
                });
            }

            user.TeacherGradeLevel = gradeType;
            user.TaughtClasses = classes.ToArray();
            if (isAdmin && user.Groups.All(g => g.GroupType != GroupType.Admin))
                user.Groups.Add(adminGroup);
            else if (!isAdmin && user.Groups.Any(g => g.GroupType == GroupType.Admin))
                user.Groups.Remove(adminGroup);
            
            user.Groups.Add(teacherGroup);

            return null;
        }
    }
}