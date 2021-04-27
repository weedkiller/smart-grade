using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/admin/administrators")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class AdministratorsApiController : UserCrudController<User>
    {
        protected override IQueryable<User> BaseQuery => database.Users
            .Include(u => u.Groups)
            .Where(u => u.Groups.Any(g => g.GroupType == GroupType.Admin));

        protected override Func<IEnumerable<User>, IEnumerable<User>> Transform => input => input;

        protected override DbSet<User> EntrySet => database.Users;
        
        protected override Func<User, dynamic> ExportTransform => item => new
        {
            Name = item.FullName,
            LoginName = item.LoginName,
            PlatformID = item.PlatformId,
            LastLogin = item.LastLogin,
        };

        public AdministratorsApiController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }

        protected override ActionResult UserCreatedOrUpdated(User user)
        {
            var adminGroup = database.Groups.Single(g => g.GroupType == GroupType.Admin);
            user.Groups.Add(adminGroup);
            return null;
        }
    }
}