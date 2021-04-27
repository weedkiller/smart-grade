using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class Url
    {
        public static string Page(this IUrlHelper _, Type pageType) => Page(pageType);

        public static string Page(Type pageType)
            => "/" + string.Join(".", pageType.FullName
                    .Substring(pageType.Assembly.GetName().Name.Length + 1)
                    .Split('.')
                    .Skip(1))
                .Replace('.', '/');
    }
}