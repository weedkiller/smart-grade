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
    [Route("api/admin/grade_levels")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class GradeLevelsApiController : CrudControllerBase<GradeLevel, BasicGradeLevelResponse>
    {
        protected override IQueryable<GradeLevel> BaseQuery => database.GradeLevels
            .Include(g => g.Groups);

        protected override Func<IEnumerable<GradeLevel>, IEnumerable<BasicGradeLevelResponse>> Transform => input => input.Select(g => new BasicGradeLevelResponse
        {
            Id = g.Id,
            Name = g.Name,
            GroupCount = g.Groups.Count,
            GradeAdded = g.EmailOnGradeAdded,
            GradeDeleted = g.EmailOnGradeDeleted,
            AbsenceAdded = g.EmailOnAbsenceAdded,
            AbsenceDeleted = g.EmailOnAbsenceDeleted,
            DisciplinaryAdded = g.EmailOnDisciplinaryAdded,
            DisciplinaryDeleted = g.EmailOnDisciplinaryDeleted
        });

        protected override DbSet<GradeLevel> EntrySet => database.GradeLevels;

        protected override Func<BasicGradeLevelResponse, dynamic> ExportTransform => item => new
        {
            Name = item.Name,
            GroupCount = item.GroupCount
        };

        public GradeLevelsApiController(AppDatabase database) : base(database, null)
        {
        }

        [HttpPost]
        [Route("create")]
        public ActionResult CreateEntry([FromForm] string name, [FromForm] int existingId, [FromForm] string emailGradeAdded, [FromForm] string emailGradeDeleted,
            [FromForm] string emailAbsenceAdded, [FromForm] string emailAbsenceDeleted, [FromForm] string emailDisciplinaryAdded, [FromForm] string emailDisciplinaryDeleted)
        {
            if (existingId == -1) // Create new
            {
                if (database.GradeLevels.Any(u => u.Name == name))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A grade level with the same name already exists."
                    });
                }

                database.GradeLevels.Add(new GradeLevel
                {
                    Name = name,
                    EmailOnGradeAdded = emailGradeAdded == "on",
                    EmailOnGradeDeleted = emailGradeDeleted == "on",
                    EmailOnAbsenceAdded = emailAbsenceAdded == "on",
                    EmailOnAbsenceDeleted = emailAbsenceDeleted == "on",
                    EmailOnDisciplinaryAdded = emailDisciplinaryAdded == "on",
                    EmailOnDisciplinaryDeleted = emailDisciplinaryDeleted == "on"
                });
                database.SaveChanges();
            }
            else // Update existing
            {
                var existingGradeLevel = database.GradeLevels.GetById(existingId);
                if (existingGradeLevel == null)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "This grade level doesn't exist anymore."
                    });
                }

                var existingOtherGradeLevel = database.GradeLevels.Any(u => u.Id != existingId && u.Name == name);
                if (existingOtherGradeLevel)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A grade level with the same name already exists."
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

                existingGradeLevel.Name = name;
                existingGradeLevel.EmailOnGradeAdded = emailGradeAdded == "on";
                existingGradeLevel.EmailOnGradeDeleted = emailGradeDeleted == "on";
                existingGradeLevel.EmailOnAbsenceAdded = emailAbsenceAdded == "on";
                existingGradeLevel.EmailOnAbsenceDeleted = emailAbsenceDeleted == "on";
                existingGradeLevel.EmailOnDisciplinaryAdded = emailDisciplinaryAdded == "on";
                existingGradeLevel.EmailOnDisciplinaryDeleted = emailDisciplinaryDeleted == "on";
                database.SaveChanges();
            }

            return Ok();
        }
    }
}