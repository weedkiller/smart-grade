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
    [Route("api/admin/grades")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class GradesApiController : CrudControllerBase<Grade, ResponseWithClass<Grade>>
    {
        protected override IQueryable<Grade> BaseQuery => database.Grades
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Include(g => g.Student)
            .ThenInclude(s => s.Groups);
        
        protected override DbSet<Grade> EntrySet => database.Grades;

        protected override Func<IEnumerable<Grade>, IEnumerable<ResponseWithClass<Grade>>> Transform => input =>
            input.Select(e => new ResponseWithClass<Grade>
            {
                Id = e.Id,
                Value = e,
                ClassId = e.Student.Groups.FirstOrDefault()?.Id,
                ClassName = e.Student.Groups.FirstOrDefault()?.Name,
            });

        protected override Func<ResponseWithClass<Grade>, dynamic> ExportTransform => item => new
        {
            Name = item.Value.Student.FullName,
            Class = item.ClassName,
            Subject = item.Value.Subject.Name,
            Grade = item.Value.Value,
            Midterm = item.Value.IsMidterm,
            Semester = item.Value.Semester,
            Date = item.Value.Date
        };

        public GradesApiController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }
    }
}