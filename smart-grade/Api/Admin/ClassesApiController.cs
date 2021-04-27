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
    [Route("api/admin/classes")]
    [Authorize(Policy = UserClaims.Administrator)]
    public class ClassesApiController : CrudControllerBase<Group, BasicGroupResponse>
    {
        protected override IQueryable<Group> BaseQuery => database.Groups
            .Include(g => g.Users)
            .Include(g => g.GradeLevel)
            .Include(g => g.FormMaster)
            .Where(g => g.GroupType == GroupType.Class);

        protected override Func<IEnumerable<Group>, IEnumerable<BasicGroupResponse>> Transform => input => input.Select(g => new BasicGroupResponse
        {
            Id = g.Id,
            Name = g.Name,
            GradeLevelId = g.GradeLevel.Id,
            GradeLevelName = g.GradeLevel.Name,
            UserCount = g.Users.Count,
            FormMasterId = g.FormMaster?.Id ?? -1,
            FormMasterName = g.FormMaster?.FullName
        });

        protected override DbSet<Group> EntrySet => database.Groups;
        
        protected override Func<BasicGroupResponse, dynamic> ExportTransform => item => new
        {
            Name = item.Name,
            UserCount = item.UserCount,
            FormMaster = item.FormMasterName,
            GradeLevel = item.GradeLevelName
        };

        public ClassesApiController(AppDatabase database) : base(database, null)
        {
        }

        [HttpPost]
        [Route("create")]
        public ActionResult CreateEntry([FromForm] string name, [FromForm] int gradeLevel, [FromForm] int existingId, [FromForm] int? formMasterId)
        {
            if (existingId == -1) // Create new
            {
                if (database.Groups.Where(g => g.GroupType == GroupType.Class).Any(u => u.Name == name))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A class with the same name already exists."
                    });
                }

                database.Groups.Add(new Group
                {
                    Name = name,
                    GroupType = GroupType.Class,
                    GradeLevel = database.GradeLevels.GetById(gradeLevel),
                    FormMaster = formMasterId != null ? database.Users.GetById(formMasterId) : null
                });
                database.SaveChanges();
            }
            else // Update existing
            {
                var existingClass = database.Groups.GetById(existingId);
                if (existingClass == null)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "This class doesn't exist anymore."
                    });
                }

                var existingOtherClass = database.Groups.Where(g => g.GroupType == GroupType.Class).Any(u => u.Id != existingId && u.Name == name);
                if (existingOtherClass)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A class with the same name already exists."
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

                existingClass.Name = name;
                existingClass.GradeLevel = database.GradeLevels.GetById(gradeLevel);
                existingClass.FormMaster = formMasterId != null ? database.Users.GetById(formMasterId) : null;
                database.SaveChanges();
            }

            return Ok();
        }
    }
}