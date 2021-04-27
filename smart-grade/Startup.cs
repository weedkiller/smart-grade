using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirestormSW.SmartGrade.Database;
using FirestormSW.SmartGrade.Database.Model;
using FirestormSW.SmartGrade.Extensions;
using FirestormSW.SmartGrade.Middlewares;
using FirestormSW.SmartGrade.Properties;
using FirestormSW.SmartGrade.Services;
using FirestormSW.SmartGrade.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Westwind.AspNetCore.LiveReload;

namespace FirestormSW.SmartGrade
{
    public class Startup
    {
        public static IWebHostEnvironment HostEnvironment { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddRazorRuntimeCompilation()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
            services.AddLiveReload(config => { config.LiveReloadEnabled = true; });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => { options.LoginPath = "/Login"; });
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.AddPolicy(UserClaims.AdministratorOrTeacher, policy => policy
                    .RequireAssertion(u =>
                        u.User.HasClaim(UserClaims.Administrator, "true") ||
                        u.User.HasClaim(UserClaims.Teacher, "true")));
                options.AddPolicy(UserClaims.Administrator, policy => policy.RequireClaim(UserClaims.Administrator));
                options.AddPolicy(UserClaims.Teacher, policy => policy.RequireClaim(UserClaims.Teacher));
                options.AddPolicy(UserClaims.Student, policy => policy.RequireClaim(UserClaims.Student));
            });
            services.AddDbContext<AppDatabase>(options =>
                options.UseSqlite("Data Source=database.sqlite3"));
            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new("en-US")
                };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CustomRequestCultureProvider(context =>
                    {
                        var loginService = context.RequestServices.GetService<LoginService>();
                        var currentUser = loginService.GetCurrentLoggedInUser(context);
                        return Task.FromResult(new ProviderCultureResult(currentUser?.PreferredLanguage ?? "en-US"));
                    })
                };
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(365);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddTransient<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();
            services.AddTransient<LoginService>();
            services.AddTransient<LanguageService>();
            services.AddTransient<EmailService>();
            services.AddTransient<DatabaseProperties>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            ClaimsPrincipal.PrimaryIdentitySelector = identities =>
            {
                var id = identities.ToArray();
                return id.FirstOrDefault(i => i.HasClaim(UserClaims.Impersonated, "true")) ??
                       id.FirstOrDefault();
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDatabase database, EmailService emailService)
        {
            HostEnvironment = env;

            app.UseLiveReload();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode >= 400 && context.Response.StatusCode <= 499 && !context.Response.HasStarted)
                {
                    context.Request.Path = "/Error";
                    await next();
                }
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseMiddleware<LanguageChangeMiddleware>();
            app.UseMiddleware<EnsureSubjectMiddleware>();
            app.UseMiddleware<SubjectChangeMiddleware>();
            app.UseMiddleware<RoleChangeMiddleware>();
            app.UseMiddleware<PhpRedirectMiddleware>();
            app.UseMiddleware<AuthCookieTimeoutMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            database.Database.EnsureDeleted();
            database.Database.EnsureCreated();

            var gradeLevelKG = database.GradeLevels.Add(new GradeLevel
            {
                Name = "Kindergarten",
                RegistryConfiguration = new RegistryConfiguration
                {
                    StartTime = TimeSpan.FromHours(8),
                    Slots = Enumerable.Range(0, 2).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasText = true
                    }).Concat(new[]
                    {
                        new RegistryTimeSlot
                        {
                            Duration = 2,
                            HasClass = true,
                            HasText = true
                        }
                    }).Concat(Enumerable.Range(0, 1).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasText = true
                    })).Concat(new[]
                    {
                        new RegistryTimeSlot
                        {
                            Duration = 2,
                            HasClass = true,
                            HasText = true
                        }
                    }).Concat(new[]
                    {
                        new RegistryTimeSlot
                        {
                            Duration = 2,
                            HasClass = true,
                            HasText = true
                        }
                    }).Concat(new[]
                    {
                        new RegistryTimeSlot
                        {
                            Duration = 3,
                            HasClass = true,
                            HasText = true,
                            CustomLabel = "Others"
                        }
                    }).ToList()
                }
            }).Entity;
            var gradeElementaryStep = database.GradeLevels.Add(new GradeLevel
            {
                Name = "Elementary (step)",
                RegistryConfiguration = new RegistryConfiguration
                {
                    StartTime = TimeSpan.FromHours(8),
                    Slots = Enumerable.Range(0, 2).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasText = true
                    }).Concat(new[]
                    {
                        new RegistryTimeSlot
                        {
                            Duration = 3,
                            HasClass = true,
                            HasText = true
                        }
                    }).Concat(Enumerable.Range(0, 5).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasText = true
                    })).ToList()
                }
            }).Entity;
            var gradeElementary = database.GradeLevels.Add(new GradeLevel
            {
                Name = "Elementary",
                RegistryConfiguration = new RegistryConfiguration
                {
                    StartTime = TimeSpan.FromHours(8),
                    Slots = Enumerable.Range(0, 10).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasSubject = true,
                        HasText = true
                    }).ToList()
                },
                EmailOnGradeAdded = true,
                EmailOnGradeDeleted = true,
                EmailOnAbsenceAdded = true,
                EmailOnAbsenceDeleted = true,
                EmailOnDisciplinaryAdded = true,
                EmailOnDisciplinaryDeleted = true
            }).Entity;
            var gradeMiddle = database.GradeLevels.Add(new GradeLevel
            {
                Name = "Middle",
                RegistryConfiguration = new RegistryConfiguration
                {
                    StartTime = TimeSpan.FromHours(7),
                    Slots = Enumerable.Range(0, 11).Select(_ => new RegistryTimeSlot
                    {
                        Duration = 1,
                        HasClass = true,
                        HasSubject = true,
                        HasText = true,
                        HasPCO = true
                    }).ToList()
                }
            }).Entity;
            database.SaveChanges();

            var adminGroup = database.Groups.Add(new Group {GroupType = GroupType.Admin, Name = "Administrators"});
            var teacherGroup = database.Groups.Add(new Group {GroupType = GroupType.Teacher, Name = "Teachers"});
            var adminUser = database.Users.Add(new User
            {
                FullName = "Administrator",
                LoginName = "admin",
                PasswordHash = "AQAAAAEAACcQAAAAEBFsY1RMVJIl9QZM2pFvdjhJQbC/OwzYV+j3JaP2hXCnhaPIyHDBPXrF3Ye3tHxAmA==",
                LastLogin = DateTime.Now,
                TeacherGradeLevel = gradeLevelKG
            });
            adminUser.Entity.Groups = new List<Group> {adminGroup.Entity, teacherGroup.Entity};
            var teacherUser = database.Users.Add(new User
            {
                FullName = "Test Teacher",
                LoginName = "tteacher1",
                PasswordHash = "AQAAAAEAACcQAAAAEBFsY1RMVJIl9QZM2pFvdjhJQbC/OwzYV+j3JaP2hXCnhaPIyHDBPXrF3Ye3tHxAmA==",
                LastLogin = DateTime.Now,
                TeacherGradeLevel = gradeElementary
            });
            teacherUser.Entity.Groups = new List<Group> {teacherGroup.Entity};

            var classGroup1 = database.Groups.Add(new Group {GroupType = GroupType.Class, GradeLevel = gradeElementary, Name = "IV.A"});
            var classGroup2 = database.Groups.Add(new Group {GroupType = GroupType.Class, GradeLevel = gradeMiddle, Name = "VII.B"});

            var s1 = database.Subjects.Add(new Subject {Name = "Programming", RegistryName = "I.T.", HasMidterm = true}).Entity;
            var s2 = database.Subjects.Add(new Subject {Name = "English", RegistryName = "Foreign language"}).Entity;

            s1.Classes = new List<Group> {classGroup1.Entity, classGroup2.Entity};
            s1.Teachers = new List<User> {adminUser.Entity};
            s2.Teachers = new List<User> {adminUser.Entity};

            adminUser.Entity.TaughtClasses = new List<Group> {classGroup1.Entity, classGroup2.Entity};
            teacherUser.Entity.TaughtClasses = new List<Group> {classGroup1.Entity, classGroup2.Entity};
            teacherUser.Entity.TaughtSubjects = new List<Subject> {s1, s2};

            classGroup1.Entity.FormMaster = adminUser.Entity;
            classGroup1.Entity.Subjects = new List<Subject> {s1, s2};
            classGroup2.Entity.Subjects = new List<Subject> {s1, s2};

            database.SaveChanges();

            var student = database.Users.Add(new User
            {
                FullName = $"Student_1",
                LoginName = $"s1",
                PasswordHash = "AQAAAAEAACcQAAAAEBFsY1RMVJIl9QZM2pFvdjhJQbC/OwzYV+j3JaP2hXCnhaPIyHDBPXrF3Ye3tHxAmA==",
                NotificationEmail = "lazar.zsolt.725@gmail.com"
            }).Entity;
            student.Groups = new List<Group> {classGroup1.Entity};

            var rand = new Random();
            for (int i = 0; i < 20; i++)
            {
                database.Grades.Add(new Grade
                {
                    Student = student,
                    Semester = rand.Next(0, 2) == 0 ? 1 : 2,
                    Teacher = adminUser.Entity,
                    Value = rand.Next(1, 10),
                    Date = DateTime.Today - TimeSpan.FromDays(rand.Next(1, 230)),
                    Subject = rand.Next(0, 2) == 0 ? s1 : s2
                });
                database.Absences.Add(new Absence
                {
                    Student = student,
                    Semester = rand.Next(0, 2) == 0 ? 1 : 2,
                    Teacher = adminUser.Entity,
                    Date = DateTime.Today - TimeSpan.FromDays(rand.Next(1, 230)),
                    Subject = rand.Next(0, 2) == 0 ? s1 : s2,
                    Comment = Guid.NewGuid().ToString(),
                    Verified = rand.Next(0, 3) == 0
                });
                database.Disciplinary.Add(new Disciplinary
                {
                    Student = student,
                    Semester = rand.Next(0, 2) == 0 ? 1 : 2,
                    Teacher = adminUser.Entity,
                    Date = DateTime.Today - TimeSpan.FromDays(rand.Next(1, 230)),
                    Subject = rand.Next(0, 2) == 0 ? s1 : s2,
                    Comment = Guid.NewGuid().ToString(),
                    Points = 0 - rand.Next(1, 11)
                });
            }

            database.DisciplinaryPresets.Add(new DisciplinaryPreset {Text = "stupid", Value = -1});
            database.DisciplinaryPresets.Add(new DisciplinaryPreset {Text = "dumb fuck", Value = -10});

            database.RegistryEntries.Add(new RegistryEntry
            {
                Class = classGroup1.Entity,
                Subject = s1,
                Teacher = adminUser.Entity,
                Text = "asdasdasd",
                EntryDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                Date = DateTime.Parse("2021-03-09 08:00:00")
            });
            database.RegistryEntries.Add(new RegistryEntry
            {
                Class = classGroup1.Entity,
                Subject = s1,
                Teacher = adminUser.Entity,
                Text = "qwe",
                EntryDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                Date = DateTime.Parse("2021-03-10 09:00:00")
            });

            var properties = new DatabaseProperties(database);

            database.SaveChanges();
            
            emailService.TryConnecting();
        }
    }
}