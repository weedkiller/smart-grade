using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Api.Responses;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    [ApiController]
    [Route("api/admin/subjects")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class SubjectsApiController : CrudControllerBase<Subject, BasicSubjectResponse>
    {
        protected override IQueryable<Subject> BaseQuery => database.Subjects
            .Include(g => g.Teachers)
            .Include(g => g.Classes)
            .AsSplitQuery();

        protected override Func<IEnumerable<Subject>, IEnumerable<BasicSubjectResponse>> Transform => input => input.Select(g => new BasicSubjectResponse
        {
            Id = g.Id,
            Name = g.Name,
            RegistryName = g.RegistryName,
            ClassCount = g.Classes.Count,
            TeacherCount = g.Teachers.Count,
            HasMidterm = g.HasMidterm,
            Teachers = g.Teachers.Select(t => new
            {
                Id = t.Id,
                FullName = t.FullName
            }),
            Classes = g.Classes.Select(c => new
            {
                Id = c.Id,
                Name = c.Name
            })
        });

        protected override DbSet<Subject> EntrySet => database.Subjects;
        
        protected override Func<BasicSubjectResponse, dynamic> ExportTransform => item => new
        {
            Name = item.Name,
            OfficialName = item.RegistryName,
            HasMidterm = item.HasMidterm
        };

        public SubjectsApiController(AppDatabase database) : base(database, null)
        {
        }

        [HttpPost]
        [Route("create")]
        public ActionResult CreateEntry([FromForm] string name, [FromForm] string regName, [FromForm] string hasMidterm,
            [FromForm] int existingId, [FromForm] int[] teachers, [FromForm] int[] classes)
        {
            if (existingId == -1) // Create new
            {
                if (database.Subjects.Any(u => u.Name == name))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A subject with the same name already exists."
                    });
                }

                database.Subjects.Add(new Subject
                {
                    Name = name,
                    RegistryName = regName,
                    HasMidterm = hasMidterm == "on",
                    Teachers = teachers.Select(i => database.Users.GetById(i)).ToList(),
                    Classes = classes.Select(i => database.Groups.GetById(i)).ToList()
                });
                database.SaveChanges();
            }
            else // Update existing
            {
                var existingSubject = database.Subjects
                    .Include(s => s.Classes)
                    .Include(s => s.Teachers)
                    .GetById(existingId);
                if (existingSubject == null)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "This subject doesn't exist anymore."
                    });
                }

                var existingOtherSubject = database.Subjects.Any(u => u.Id != existingId && u.Name == name);
                if (existingOtherSubject)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A subject with the same name already exists."
                    });
                }

                if (string.IsNullOrEmpty(name.Trim()))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "The name can't be empty."
                    });
                }

                existingSubject.Name = name;
                existingSubject.RegistryName = regName;
                existingSubject.HasMidterm = hasMidterm == "on";
                existingSubject.Teachers?.Clear();
                existingSubject.Classes?.Clear();
                database.SaveChanges();
                existingSubject.Teachers = teachers.Select(i => database.Users.GetById(i)).ToList();
                existingSubject.Classes = classes.Select(i => database.Groups.GetById(i)).ToList();
                database.SaveChanges();
            }

            return Ok();
        }
    }
}