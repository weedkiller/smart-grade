using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FirestormSW.SmartGrade.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredClaimsAttribute : ActionFilterAttribute
    {
        public IEnumerable<string> Claims { get; set; }

        public RequiredClaimsAttribute(params string[] claims)
        {
            Claims = claims;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Console.Out.WriteLine(Claims.Count());
            base.OnActionExecuting(context);
        }
    }
}