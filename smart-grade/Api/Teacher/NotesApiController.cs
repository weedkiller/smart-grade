using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Teacher
{
    [ApiController]
    [Route("api/teacher/notes")]
    [Authorize(Policy = UserClaims.Teacher)]
    public class NotesApiController : ControllerBase
    {
        private readonly AppDatabase database;
        private readonly LoginService loginService;

        public NotesApiController(AppDatabase database, LoginService loginService)
        {
            this.database = database;
            this.loginService = loginService;
        }

        [HttpGet]
        [Route("user_notes")]
        public ActionResult GetDisciplinaryReports(int studentId, int subjectId)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            return Ok(database.Notes
                .Include(g => g.Student)
                .Include(g => g.Teacher)
                .Include(g => g.Subject)
                .Where(n => n.Student.Id == studentId && n.Teacher.Id == teacher.Id && n.Subject.Id == subjectId)
                .Select(g => new
                {
                    Id = g.Id,
                    Text = g.Text,
                    StudentId = g.Student.Id,
                    TeacherId = g.Teacher.Id,
                    SubjectId = g.Subject.Id
                }).SingleOrDefault());
        }

        [HttpPost]
        public ActionResult CreateOrUpdateNote([FromForm] int studentId, [FromForm] int subjectId, [FromForm] string text)
        {
            var teacher = loginService.GetCurrentLoggedInUser(HttpContext);
            var note = database.Notes
                           .Include(n => n.Student)
                           .Include(n => n.Subject)
                           .Include(n => n.Teacher)
                           .SingleOrDefault(n => n.Student.Id == studentId && n.Teacher.Id == teacher.Id && n.Subject.Id == subjectId)
                       ?? new Note();
            note.Text = text;
            note.Student = database.Users.GetById(studentId);
            note.Teacher = database.Users.GetById(teacher.Id);
            note.Subject = database.Subjects.GetById(subjectId);
            if (!database.Notes.Contains(note))
                database.Notes.Add(note);
            database.SaveChanges();
            return Ok();
        }
    }
}