using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Utils
{
    public static class TeacherUtils
    {
        public static IEnumerable<Group> GetOrderedClasses(AppDatabase database, User currentUser)
        {
            return (from g in database.Groups
                join h in database.TeacherClassHistory
                        .Include(h => h.Class)
                        .Include(h => h.Teacher)
                    on g.Id equals h.Class.Id into j1
                from h in j1.DefaultIfEmpty()
                orderby h.Id descending
                where g.GroupType == GroupType.Class
                where currentUser.TaughtClasses.Contains(g)
                select g).ToList();
        }
    }
}