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
    [Route("api/admin/disciplinary")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class DisciplinaryApiController : CrudControllerBase<Disciplinary, ResponseWithClass<Disciplinary>>
    {
        protected override IQueryable<Disciplinary> BaseQuery => database.Disciplinary
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Include(g => g.Student)
            .ThenInclude(s => s.Groups);

        protected override Func<IEnumerable<Disciplinary>, IEnumerable<ResponseWithClass<Disciplinary>>> Transform => input =>
            input.Select(e => new ResponseWithClass<Disciplinary>
            {
                Id = e.Id,
                Value = e,
                ClassId = e.Student.Groups.FirstOrDefault()?.Id,
                ClassName = e.Student.Groups.FirstOrDefault()?.Name,
            });

        protected override DbSet<Disciplinary> EntrySet => database.Disciplinary;
        
        protected override Func<ResponseWithClass<Disciplinary>, dynamic> ExportTransform => item => new
        {
            Name = item.Value.Student.FullName,
            Class = item.ClassName,
            Subject = item.Value.Subject.Name,
            Semester = item.Value.Semester,
            Points = item.Value.Points,
            Comment = item.Value.Comment,
            Date = item.Value.Date
        };

        public DisciplinaryApiController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }
    }
}