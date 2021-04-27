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
    [Route("api/teacher/absences")]
    [Authorize(Policy = UserClaims.Teacher)]
    public class AbsencesApiController : ControllerBase, ISummaryProvider
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;
        private readonly EmailService emailService;

        public AbsencesApiController(AppDatabase database, LoginService loginService, EmailService emailService)
        {
            this.database = database;
            this.loginService = loginService;
            this.emailService = emailService;
        }

        [HttpGet]
        [Route("user_absences")]
        public ActionResult GetAbsences(int studentId, int subjectId, int teacherId)
        {
            return Ok(database.Absences
                .Include(g => g.Student)
                .Include(g => g.Teacher)
                .Include(g => g.Subject)
                .OrderBy(g => g.Date)
                .Select(g => new
                {
                    Id = g.Id,
                    Comment = g.Comment,
                    Date = g.Date.ToString("yyyy. MM. dd. HH:mm"),
                    Semester = g.Semester,
                    Verified = g.Verified,
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
        public ActionResult CreateAbsence([FromForm] int studentId, [FromForm] int subjectId, [FromForm] string comment,
            [FromForm] int semester, [FromForm] DateTime date)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            var student = database.Users.GetById(studentId);
            var absence = new Absence
            {
                Comment = comment,
                Date = date,
                Semester = semester,
                Student = student,
                Teacher = database.Users.GetById(teacher.Id),
                Subject = database.Subjects.GetById(subjectId)
            };
            database.Absences.Add(absence);
            database.SaveChanges();
            
            emailService.SendNotification(absence, true);
            
            return Ok(GetSummary(student, absence.Subject));
        }

        [HttpPost]
        [Route("toggle_verify")]
        public ActionResult ToggleVerify([FromForm] int absenceId, [FromForm] string verified)
        {
            var absence = database.Absences
                .Include(a => a.Student)
                .Include(a => a.Subject)
                .GetById(absenceId);
            if (database.Absences.Contains(absence))
            {
                absence.Verified = verified == "on";
                database.SaveChanges();
                return Ok(GetSummary(absence.Student, absence.Subject));
            }

            return BadRequest();
        }

        [HttpDelete]
        public ActionResult DeleteAbsence([FromForm] int absenceId)
        {
            var absence = database.Absences
                .Include(a => a.Student)
                .Include(a => a.Subject)
                .GetById(absenceId);
            database.Absences.Remove(absence);
            database.SaveChanges();
            
            emailService.SendNotification(absence, false);
            
            return Ok(GetSummary(absence.Student, absence.Subject));
        }

        [NonAction]
        public string GetSummary(User user, Subject subject)
        {
            var absences = database.Absences
                .Include(a => a.Student)
                .Where(a => a.Student.Id == user.Id && (subject == null || a.Subject.Id == subject.Id))
                .Where(a => !a.Verified)
                .AsEnumerable()
                .GroupBy(g => g.Semester);

            string result = "";

            foreach (var semester in absences)
                result += $"Semester {semester.Key}: {semester.Count()}; "; // TODO localize

            return result.Trim(' ', ';');
        }
    }
}