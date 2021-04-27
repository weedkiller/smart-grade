using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Attributes;
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
    [Route("api/teacher/grades")]
    [Authorize(Policy = UserClaims.Teacher)]
    [RequiredClaims(UserClaims.Teacher)]
    public class GradesApiController : ControllerBase, ISummaryProvider
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;
        private readonly EmailService emailService;

        public GradesApiController(AppDatabase database, LoginService loginService, EmailService emailService)
        {
            this.database = database;
            this.loginService = loginService;
            this.emailService = emailService;
        }

        [HttpGet]
        [Route("user_grades")]
        [RequiredClaims(UserClaims.Teacher, UserClaims.Administrator)]
        public ActionResult GetGrades(int studentId, int subjectId, int teacherId)
        {
            return Ok(database.Grades
                .Include(g => g.Student)
                .Include(g => g.Teacher)
                .Include(g => g.Subject)
                .OrderBy(g => g.Date)
                .Select(g => new
                {
                    Id = g.Id,
                    Value = g.Value,
                    Date = g.Date.ToString("yyyy. MM. dd. HH:mm"),
                    Semester = g.Semester,
                    StudentId = g.Student.Id,
                    TeacherId = g.Teacher.Id,
                    SubjectId = g.Subject.Id,
                    SubjectName = g.Subject.Name,
                    IsMidterm = g.IsMidterm
                })
                .QueryPaged(new QueryCollection(new Dictionary<string, StringValues>
                {
                    {"pagination[perpage]", "100"},
                    {"query[StudentId]", $"{studentId}"},
                    //{"query[TeacherId]", $"{teacherId}"},
                    //{"query[SubjectId]", $"{subjectId}"},
                    {"sort[field]", "Semester"},
                    {"sort[sort]", "ASC"}
                }), input => input
                    .Where(i => i.SubjectId == subjectId || subjectId == -2).OrderBy(i => i.SubjectId)));
        }

        [HttpPost]
        public ActionResult CreateGrade([FromForm] int studentId, [FromForm] int subjectId, [FromForm] int value, [FromForm] int semester, [FromForm] DateTime date,
            [FromForm] string isMidterm)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            var student = database.Users.GetById(studentId);
            var grade = new Grade
            {
                Value = value,
                Date = date,
                Semester = semester,
                IsMidterm = isMidterm == "on",
                Student = student,
                Teacher = database.Users.GetById(teacher.Id),
                Subject = database.Subjects.GetById(subjectId)
            };
            database.Grades.Add(grade);
            database.SaveChanges();
            
            emailService.SendNotification(grade, true);
            
            return Ok(GetSummary(student, grade.Subject));
        }

        [HttpDelete]
        public ActionResult DeleteGrade([FromForm] int gradeId)
        {
            var grade = database.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .GetById(gradeId);
            database.Grades.Remove(grade);
            database.SaveChanges();
            
            emailService.SendNotification(grade, false);
            
            return Ok(GetSummary(grade.Student, grade.Subject));
        }

        [NonAction]
        public string GetSummary(User user, Subject subject)
        {
            if (subject == null)
                return null;
            var grades = database.Grades
                .Include(g => g.Student)
                .Where(g => g.Student.Id == user.Id && g.Subject.Id == subject.Id)
                .AsEnumerable()
                .GroupBy(g => g.Semester);
            string result = "";
            foreach (var semester in grades)
                result += $"Semester {semester.Key}: " +
                          $"{string.Join(", ", semester.Select(s => s.Value))}, " +
                          $"Average: {AverageCalculator.GetAverageForSubject(subject, semester.Key, semester.ToList()):F2}; "; // TODO localize

            return result.Trim(' ', ';');
        }
    }
}