using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace FirestormSW.SmartGrade.Api.Teacher
{
    [ApiController]
    [Route("api/teacher/disciplinary")]
    [Authorize(Policy = UserClaims.Teacher)]
    public class DisciplinaryApiController : ControllerBase, ISummaryProvider
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;
        private readonly EmailService emailService;

        public DisciplinaryApiController(AppDatabase database, LoginService loginService, EmailService emailService)
        {
            this.database = database;
            this.loginService = loginService;
            this.emailService = emailService;
        }

        [HttpGet]
        [Route("user_disciplinary")]
        public ActionResult GetDisciplinaryReports(int studentId, int subjectId, int teacherId)
        {
            return Ok(database.Disciplinary
                .Include(g => g.Student)
                .Include(g => g.Teacher)
                .Include(g => g.Subject)
                .OrderBy(g => g.Date)
                .Select(g => new
                {
                    Id = g.Id,
                    Comment = g.Comment,
                    Points = g.Points,
                    Date = g.Date.ToString("yyyy. MM. dd. HH:mm"),
                    Semester = g.Semester,
                    StudentId = g.Student.Id,
                    TeacherId = g.Teacher.Id,
                    SubjectId = g.Subject.Id,
                    SubjectName = g.Subject.Name
                })
                .QueryPaged(new QueryCollection(new Dictionary<string, StringValues>
                {
                    {"pagination[perpage]", "10000"},
                    {"query[StudentId]", $"{studentId}"},
                    //{"query[TeacherId]", $"{teacherId}"},
                    //{"query[SubjectId]", $"{subjectId}"},
                    {"sort[field]", "Semester"},
                    {"sort[sort]", "ASC"},
                }), input => input
                    .Where(i => i.SubjectId == subjectId || subjectId == -2).OrderBy(i => i.SubjectId)));
        }

        [HttpPost]
        public ActionResult CreateDisciplinaryReport([FromForm] int studentId, [FromForm] int subjectId, [FromForm] string comment,
            [FromForm] int points, [FromForm] int semester, [FromForm] DateTime date)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            var student = database.Users.GetById(studentId);
            var disciplinaryReport = new Disciplinary
            {
                Comment = comment,
                Points = points,
                Date = date,
                Semester = semester,
                Student = student,
                Teacher = database.Users.GetById(teacher.Id),
                Subject = database.Subjects.GetById(subjectId)
            };
            database.Disciplinary.Add(disciplinaryReport);
            database.SaveChanges();
            
            emailService.SendNotification(disciplinaryReport, true);
            
            return Ok(GetSummary(student, disciplinaryReport.Subject));
        }

        [HttpDelete]
        public ActionResult DeleteDisciplinaryReport([FromForm] int disciplinaryId)
        {
            var disciplinaryReport = database.Disciplinary
                .Include(d => d.Student)
                .Include(d => d.Subject)
                .GetById(disciplinaryId);
            database.Disciplinary.Remove(disciplinaryReport);
            database.SaveChanges();
            
            emailService.SendNotification(disciplinaryReport, false);
            
            return Ok(GetSummary(disciplinaryReport.Student, disciplinaryReport.Subject));
        }

        [NonAction]
        public string GetSummary(User user, Subject subject)
        {
            var disciplinary = database.Disciplinary
                .Include(a => a.Student)
                .Where(a => a.Student.Id == user.Id && (subject == null || a.Subject.Id == subject.Id))
                .AsEnumerable()
                .GroupBy(g => g.Semester);

            string result = "";

            foreach (var semester in disciplinary)
                result += $"Semester {semester.Key}: {semester.Sum(d => d.Points)}; "; // TODO localize

            return result.Trim(' ', ';');
        }
    }
}