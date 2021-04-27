using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Api.Responses;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/students")]
    [Authorize(Policy = UserClaims.Administrator)]
    [AllowAnonymous]
    public class StudentsApiController : UserCrudController<BasicUserResponse>
    {
        protected override DbSet<User> EntrySet => database.Users;

        public StudentsApiController(AppDatabase database) : base(database, null)
        {
        }

        protected override IQueryable<User> BaseQuery => database.Users
            .Include(u => u.Groups)
            .ThenInclude(g => g.GradeLevel)
            .Where(u => u.Groups.Any(g => g.GroupType == GroupType.Class));

        protected override Func<IEnumerable<User>, IEnumerable<BasicUserResponse>> Transform => input => input
            .Select(u => new BasicUserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                LoginName = u.LoginName,
                NotificationEmail = u.NotificationEmail,
                PlatformID = u.PlatformId,
                PreferredLanguage = u.PreferredLanguage,
                LastLogin = u.LastLogin,
                ClassId = u.Groups.FirstOrDefault()?.Id,
                ClassName = u.Groups.FirstOrDefault()?.Name,
                HasPassword = u.HasPassword
            });
        
        protected override Func<BasicUserResponse, dynamic> ExportTransform => item => new
        {
            Name = item.FullName,
            LoginName = item.LoginName,
            PlatformID = item.PlatformID,
            Class = item.ClassName,
            LastLogin = item.LastLogin,
        };

        protected override ActionResult UserCreatedOrUpdated(User user)
        {
            Group selectedClass = null;
            if (int.TryParse(Request.Form["group"], out int groupId))
                selectedClass = database.Groups.SingleOrDefault(g => g.Id == groupId);
            if (selectedClass == null)
            {
                return BadRequest(new
                {
                    Error = true,
                    Message = "This class does not exist."
                });
            }

            var classGroups = user.Groups.Where(g => g.GroupType == GroupType.Class).ToArray();
            foreach (var group in classGroups)
                user.Groups.Remove(group);

            user.Groups.Add(selectedClass);

            return null;
        }
    }
}