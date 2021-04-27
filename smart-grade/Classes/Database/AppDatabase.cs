using System.Linq;
using FirestormSW.SmartGrade.Database.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirestormSW.SmartGrade.Database
{
    public class AppDatabase : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GradeLevel> GradeLevels { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<Disciplinary> Disciplinary { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<RegistryEntry> RegistryEntries { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<DisciplinaryPreset> DisciplinaryPresets { get; set; }
        public DbSet<RegistryClassHistory> RegistryClassHistory { get; set; }
        public DbSet<TeacherClassHistory> TeacherClassHistory { get; set; }

        public AppDatabase(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSnakeCaseNamingConvention();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>()
                .HasOne(g => g.GradeLevel)
                .WithMany(g => g.Groups)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasMany(g => g.Users)
                .WithMany(g => g.Groups);

            modelBuilder.Entity<User>()
                .HasOne(u => u.CurrentRole)
                .WithMany();

            modelBuilder.Entity<User>()
                .HasMany(u => u.TaughtClasses)
                .WithMany(g => g.Teachers)
                .UsingEntity(j => j.ToTable("teacher_class"));

            modelBuilder.Entity<Group>()
                .HasOne(g => g.FormMaster)
                .WithMany();

            modelBuilder.Entity<RegistryTimeSlot>()
                .Property(e => e.Presets)
                .HasConversion(
                    v => JArray.FromObject(v).ToString(Formatting.None),
                    v => JArray.Parse(v).Values<string>().ToList());

            var stringProperties = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string));
            foreach (var property in stringProperties)
            {
                if (property.GetColumnType() == null)
                    property.SetColumnType("text");
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                entityType.SetTableName($"sg_{tableName}");
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}