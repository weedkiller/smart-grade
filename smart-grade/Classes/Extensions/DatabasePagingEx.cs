using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Paging;
using Microsoft.AspNetCore.Http;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class DatabasePagingEx
    {
        public static PagedObject<T> QueryPaged<T>(this IQueryable<T> baseQuery, IQueryCollection query)
            => QueryPaged(baseQuery, query, input => input);

        public static PagedObject<TTransform> QueryPaged<T, TTransform>(this IQueryable<T> baseQuery, IQueryCollection query,
            Func<IEnumerable<T>, IEnumerable<TTransform>> transform)
        {
            var paginationPage = int.Parse(query["pagination[page]"].FirstOrDefault().OrNull() ?? "1");
            var paginationPerPage = int.Parse(query["pagination[perpage]"].FirstOrDefault().OrNull() ?? "35");

            var sortField = query["sort[field]"].FirstOrDefault();
            var sortDirection = query["sort[sort]"].FirstOrDefault()?.ToUpper();

            var genericSearch = query["query[generalSearch]"].FirstOrDefault();

            if (!string.IsNullOrEmpty(genericSearch))
            {
                var conditions = new List<string>();
                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    if (propertyInfo.PropertyType != typeof(string))
                        continue;

                    if (propertyInfo.GetCustomAttribute(typeof(SearchableAttribute)) == null)
                        continue;

                    conditions.Add($"{propertyInfo.Name}.Contains(@0)");
                }

                if (conditions.Any())
                    baseQuery = baseQuery.Where(string.Join(" || ", conditions), genericSearch);
            }

            var transformedQuery = transform(baseQuery).AsQueryable();

            foreach (var (key, value) in query.Where(q => q.Key.StartsWith("query[")))
            {
                string propertyName = key.Split('[', ']')[1];
                var property = GetPropType(propertyName, transformedQuery.ElementType);

                if (propertyName == "generalSearch")
                    continue;

                if (property == typeof(string))
                    transformedQuery = transformedQuery.Where($"{propertyName}.Contains(@0)", value);
                else if (property == typeof(DateTime))
                {
                    var startDate = DateTime.Parse(value.ToString().Split('-')[0].Trim());
                    var endDate = DateTime.Parse(value.ToString().Split('-')[1].Trim());
                    transformedQuery = transformedQuery.Where($"{propertyName} >= @0 && {propertyName} <= @1", startDate, endDate);
                }
                else
                    transformedQuery = transformedQuery.Where($"{propertyName} == @0", value);
            }

            if (sortField != null && sortDirection != null && "ASC DESC".Contains(sortDirection) && GetPropType(sortField, transformedQuery.ElementType) != null)
                transformedQuery = transformedQuery.OrderBy($"{sortField} {sortDirection}");
            else
                transformedQuery = transformedQuery.OrderBy("Id ASC");
            
            var totalCount = transformedQuery.Count();
            var pageView = transformedQuery
                .Skip((paginationPage - 1) * paginationPerPage)
                .Take(paginationPerPage)
                .AsQueryable();
            
            // Summarizing
            var sumKey = query["summarizer"].FirstOrDefault();
            float sumValue = 0;
            if (!string.IsNullOrEmpty(sumKey))
                sumValue = (float)Convert.ChangeType(transformedQuery.Sum(sumKey), typeof(float));

            return new PagedObject<TTransform>
            {
                Meta = new PagedObjectMeta
                {
                    Page = paginationPage,
                    Pages = (int) Math.Ceiling(totalCount / (float) paginationPerPage),
                    PerPage = paginationPerPage,
                    Total = totalCount,
                    Field = sortField,
                    Sort = sortDirection,
                    Sum = sumValue
                },
                Data = pageView.ToList()
            };
        }
        
        private static Type GetPropType(string name, Type type)
        {
            foreach (var part in name.Split('.'))
            {
                var info = type.GetProperty(part);
                if (info == null)
                    return null;

                type = info.PropertyType;
            }

            return type;
        }
    }
}