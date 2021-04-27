using System.Linq;
using FirestormSW.SmartGrade.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class DbSetEx
    {
        public static T GetById<T>(this DbSet<T> entrySet, int? id) where T : class, IEntryWithKey
            => entrySet.SingleOrDefault(e => e.Id == id);

        public static T GetById<T>(this IQueryable<T> entrySet, int? id) where T : class, IEntryWithKey
            => entrySet.SingleOrDefault(e => e.Id == id);
    }
}