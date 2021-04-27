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
    [Route("api/admin/absences")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class AbsencesApiController : CrudControllerBase<Absence, ResponseWithClass<Absence>>
    {
        protected override IQueryable<Absence> BaseQuery => database.Absences
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Include(g => g.Student)
            .ThenInclude(s => s.Groups);

        protected override Func<IEnumerable<Absence>, IEnumerable<ResponseWithClass<Absence>>> Transform => input => 
            input.Select(e => new ResponseWithClass<Absence>
        {
            Id = e.Id,
            Value = e,
            ClassId = e.Student.Groups.FirstOrDefault()?.Id,
            ClassName = e.Student.Groups.FirstOrDefault()?.Name,
        });

        protected override DbSet<Absence> EntrySet => database.Absences;
        
        protected override Func<ResponseWithClass<Absence>, dynamic> ExportTransform => item => new
        {
            Name = item.Value.Student.FullName,
            Class = item.ClassName,
            Subject = item.Value.Subject.Name,
            Verified = item.Value.Verified,
            Semester = item.Value.Semester,
            Comment = item.Value.Comment,
            Date = item.Value.Date
        };

        public AbsencesApiController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }
    }
}