using System;
using System.Collections.Generic;
using System.Linq;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Api.Admin
{
    public abstract class UserCrudController<TTransform> : CrudControllerBase<User, TTransform>
    {
        protected override DbSet<User> EntrySet => database.Users;

        protected UserCrudController(AppDatabase database, LoginService loginService) : base(database, loginService)
        {
        }
        
        [HttpPost]
        [Route("create")]
        public ActionResult CreateEntry([FromForm] string fullName, [FromForm] string loginName, [FromForm] string password, 
            [FromForm] int existingId, [FromForm] string notificationEmail)
        {
            if (existingId == -1) // Create new
            {
                if (database.Users.Any(u => u.LoginName == loginName))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A user with the same login name already exists."
                    });
                }

                try
                {
                    var hasher = new PasswordHasher<User>();
                    var user = database.Users.Add(new User
                    {
                        Groups = new List<Group>(),
                        FullName = fullName,
                        LoginName = loginName,
                        NotificationEmail = notificationEmail
                    });
                    var result = UserCreatedOrUpdated(user.Entity);
                    if (result != null)
                        return result;
                    if (!string.IsNullOrEmpty(password))
                        user.Entity.PasswordHash = hasher.HashPassword(null, password);
                    database.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = ex.Message
                    });
                }
            }
            else // Update existing
            {
                var user = database.Users
                    .Include(u => u.Groups)
                    .Include(u => u.TaughtSubjects)
                    .SingleOrDefault(u => u.Id == existingId);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "This user doesn't exist anymore."
                    });
                }

                var existingOtherUser = database.Users.Any(u => u.Id != existingId && u.LoginName == loginName);
                if (existingOtherUser)
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "A user with the same login name already exists."
                    });
                }

                if (string.IsNullOrEmpty(fullName.Trim()))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "The full name can't be empty."
                    });
                }

                if (string.IsNullOrEmpty(loginName.Trim()))
                {
                    return BadRequest(new
                    {
                        Error = true,
                        Message = "The login name can't be empty."
                    });
                }

                var hasher = new PasswordHasher<User>();
                user.FullName = fullName;
                user.LoginName = loginName;
                user.NotificationEmail = notificationEmail;
                user.Groups ??= new List<Group>();
                var result = UserCreatedOrUpdated(user);
                if (result != null)
                    return result;
                if (!string.IsNullOrEmpty(password) && password != "**********")
                    user.PasswordHash = hasher.HashPassword(null, password);
                database.SaveChanges();
            }

            return Ok();
        }

        protected abstract ActionResult UserCreatedOrUpdated(User user);
    }
}