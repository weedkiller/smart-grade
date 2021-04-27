using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Export;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    public abstract class CrudControllerBase<TEntry, TTransform> : ControllerBase, IDataProvider where TEntry : class, IEntryWithKey
    {
        protected readonly AppDatabase database;
        protected readonly LoginService loginService;

        protected abstract IQueryable<TEntry> BaseQuery { get; }
        protected abstract Func<IEnumerable<TEntry>, IEnumerable<TTransform>> Transform { get; }
        protected virtual Func<TTransform, dynamic> ExportTransform => input => input;
        protected abstract DbSet<TEntry> EntrySet { get; }

        protected CrudControllerBase(AppDatabase database, LoginService loginService)
        {
            this.database = database;
            this.loginService = loginService;
        }

        public IEnumerable<object> GetData(IQueryCollection queryCollection)
        {
            var dictionary = queryCollection.ToDictionary(k => k.Key, v => v.Value);
            dictionary["pagination[perpage]"] = $"{int.MaxValue}";
            return BaseQuery.QueryPaged(new QueryCollection(dictionary), Transform).Data
                .Select(i => ExportTransform(i));
        }

        public virtual IEnumerable<string> GetHeaders() => Enumerable.Empty<string>();
        public virtual string GetTitle() => GetType().Name;

        [HttpGet]
        [Route("list")]
        public ActionResult ListResource()
        {
            return Ok(BaseQuery.QueryPaged(Request.Query, Transform));
        }

        [HttpPost]
        [Route("delete")]
        public ActionResult DeleteEntry([FromForm] int id)
        {
            var entry = EntrySet.SingleOrDefault(u => u.Id == id);
            if (entry == null)
            {
                return BadRequest(new
                {
                    Error = true,
                    Message = "This user doesn't exist anymore."
                });
            }

            if (entry is User && entry.Id == loginService.GetCurrentLoggedInUser(HttpContext).Id)
            {
                return BadRequest(new
                {
                    Error = true,
                    Message = "Cannot delete the current user."
                });
            }

            EntrySet.Remove(entry);
            database.SaveChanges();

            return Ok();
        }
    }
}