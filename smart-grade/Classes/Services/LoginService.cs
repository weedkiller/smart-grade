using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirestormSW.SmartGrade.Services
{
    public class LoginService
    {
        public static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(15);

        private readonly AppDatabase database;

        public LoginService(AppDatabase database)
        {
            this.database = database;
        }

        public Task<bool> SignInWithCredentials(HttpContext context, string username, string password)
        {
            var user = database.Users
                .Include(u => u.Groups)
                .SingleOrDefault(u => u.LoginName == username);
            if (user == null)
                return Task.FromResult(false);

            var passwordHasher = new PasswordHasher<User>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Success)
                return Task.FromResult(false);

            string returnUrl = context.Request.Form["ReturnUrl"];
            if (string.IsNullOrEmpty(returnUrl) || returnUrl.Trim(' ', '/') == "Logout" || returnUrl.Trim(' ', '/') == "Login")
                returnUrl = null;

            user.LastLogin = DateTime.Now;

            database.SaveChanges();

            string startPage = GetStartingPage(user);
            if (startPage == null)
                Console.Out.WriteLine("This user doesn't belong to a group, can't log in");

            var claimsIdentity = new ClaimsIdentity(GetClaimsForUser(user), CookieAuthenticationDefaults.AuthenticationScheme);
            return SignInWithClaims(context, new[] {claimsIdentity}, returnUrl ?? startPage)
                .ContinueWith(_ => true);
        }

        private static Task SignInWithClaims(HttpContext context, IEnumerable<ClaimsIdentity> identities, string startPage)
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = startPage,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(SessionTimeout),
                IsPersistent = true
            };
            return context
                .SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identities),
                    authenticationProperties);
        }

        private static IEnumerable<Claim> GetClaimsForUser(User user)
        {
            var claims = new List<Claim>
            {
                new(UserClaims.UserName, user.LoginName),
                new(UserClaims.UserId, user.Id.ToString())
            };

            var adminGroup = user.Groups.FirstOrDefault(g => g.GroupType == GroupType.Admin);
            var teacherGroup = user.Groups.FirstOrDefault(g => g.GroupType == GroupType.Teacher);
            var classGroup = user.Groups.FirstOrDefault(g => g.GroupType == GroupType.Class);

            if (classGroup != null)
            {
                claims.Add(new Claim(UserClaims.Student, "true"));
                user.CurrentRole = classGroup;
            }

            if (teacherGroup != null)
            {
                claims.Add(new Claim(UserClaims.Teacher, "true"));
                user.CurrentRole = teacherGroup;
            }

            if (adminGroup != null)
            {
                claims.Add(new Claim(UserClaims.Administrator, "true"));
                user.CurrentRole = adminGroup;
            }

            return claims;
        }

        public Task SignOut(HttpContext context) => context.SignOutAsync();

        public string GetStartingPage(User user)
        {
            if (user.CurrentRole == null)
            {
                user.CurrentRole = user.Groups.OrderBy(g => g.GroupType).LastOrDefault();
                database.SaveChanges();
            }

            if (user.CurrentRole.GroupType == GroupType.Admin)
                return "/Admin/Administrators";

            if (user.CurrentRole.GroupType == GroupType.Teacher)
                return "/Teacher/Grades"; // TODO check in registry settings

            if (user.CurrentRole.GroupType == GroupType.Class)
                return "/Student/Grades";

            return null;
        }

        public User GetCurrentLoggedInUser(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
                return null;

            var identity =
                (context.User.Identities.FirstOrDefault(i => i.HasClaim(UserClaims.Impersonated, "true"))
                 ?? context.User.Identity) as ClaimsIdentity;

            return identity.GetDbUser(database);
        }

        public async void ImpersonateUser(HttpContext context, User user)
        {
            var newIdentity = new ClaimsIdentity(GetClaimsForUser(user), CookieAuthenticationDefaults.AuthenticationScheme);
            newIdentity.AddClaim(new Claim(UserClaims.Impersonated, "true"));
            context.User.AddIdentity(newIdentity);
            await SignInWithClaims(context, context.User.Identities, GetStartingPage(user));
            //context.Response.Redirect(GetStartingPage(user));
        }

        public async void StopImpersonatingUser(HttpContext context)
        {
            var originalIdentity = context.User.Identities.First(i => !i.HasClaim(UserClaims.Impersonated, "true"));
            var originalUser = originalIdentity.GetDbUser(database);
            await SignInWithClaims(context, new[] {originalIdentity}, GetStartingPage(originalUser));
            context.Response.Redirect(GetStartingPage(originalUser));
        }
    }
}