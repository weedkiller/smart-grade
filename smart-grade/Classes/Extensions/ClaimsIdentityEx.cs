using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Utils;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class ClaimsIdentityEx
    {
        public static User GetDbUser(this ClaimsIdentity identity, AppDatabase database)
        {
            if (!int.TryParse(identity.Claims.First(c => c.Type == UserClaims.UserId).Value, out int userId))
                return null;

            return database.Users
                .Include(u => u.Groups)
                .ThenInclude(g => g.FormMaster)
                .Include(u => u.TaughtClasses)
                .Include(u => u.TaughtSubjects)
                .Include(u => u.CurrentRole)
                .ThenInclude(r => r.GradeLevel)
                .Include(u => u.TeacherGradeLevel)
                .AsSplitQuery()
                .SingleOrDefault(u => u.Id == userId);
        }

        public static bool IsImpersonated(this IIdentity identity)
            => identity is ClaimsIdentity claimsIdentity && claimsIdentity.HasClaim(UserClaims.Impersonated, "true");
    }
}