using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        
        public DbSet<School> Schools { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<SubjectStudent> SubjectStudents { get; set; }
        public DbSet<StudentClass> StudentClass { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Year> Years { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Salary> Salarys { get; set; }
        public DbSet<TeacherStudent> TeacherStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call the base method
            
            modelBuilder.Entity<TeacherStudent>().HasKey(TS => new { TS.StudentID, TS.TeacherID });
            modelBuilder.Entity<StudentClass>().HasKey(SC => new { SC.StudentID, SC.ClassID });
            modelBuilder.Entity<SubjectStudent>().HasKey(SS => new { SS.SubjectID, SS.StudentID });
            // Ternary relationship between Teachers, Students, and Subjects
            modelBuilder.Entity<TeacherSubjectStudent>()
                .HasKey(tss => new { tss.TeacherID, tss.StudentID, tss.SubjectID });

            modelBuilder.Entity<School>()
                .HasOne<Manager>(m => m.Manager)
                .WithOne(s => s.School)
                .HasForeignKey<Manager>(m => m.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            // many to many relationship for Teachers and Students
            modelBuilder.Entity<TeacherStudent>()
                .HasOne<Student>(S => S.Student)
                .WithMany(TS => TS.TeacherStudents)
                .HasForeignKey(S => S.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TeacherStudent>()
                .HasOne<Teacher>(T => T.Teacher)
                .WithMany(TS => TS.TeacherStudents)
                .HasForeignKey(T => T.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many for School and Year
            modelBuilder.Entity<School>()
                .HasMany<Year>(s => s.Years)
                .WithOne(Y => Y.School)
                .HasForeignKey(Y => Y.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many for Year and Stage
            modelBuilder.Entity<Year>()
                .HasMany<Stage>(Y => Y.Stages)
                .WithOne(P => P.Year)
                .HasForeignKey(P => P.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many for Stage and Class
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Stage)
                .WithMany(p => p.Classes)
                .HasForeignKey(c => c.StageID)
                .OnDelete(DeleteBehavior.Restrict);


            // one to many for Managers and Teachers
            modelBuilder.Entity<Manager>()
                .HasMany<Teacher>(T => T.Teachers)
                .WithOne(M => M.Manager)
                .HasForeignKey(T => T.ManagerID)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent deletion of Teachers when Manager is deleted

            // many to many relationship for Subjects and Students
            modelBuilder.Entity<SubjectStudent>()
                .HasOne<Subject>(S => S.Subject)
                .WithMany(SS => SS.SubjectStudents)
                .HasForeignKey(S => S.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SubjectStudent>()
                .HasOne<Student>(S => S.Student)
                .WithMany(SS => SS.SubjectStudents)
                .HasForeignKey(S => S.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // many to many relationship for Classes and Students
            modelBuilder.Entity<StudentClass>()
                .HasOne<Student>(S => S.Student)
                .WithMany(SC => SC.StudentClass)
                .HasForeignKey(S => S.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentClass>()
                .HasOne<Class>(C => C.Class)
                .WithMany(SC => SC.StudentClass)
                .HasForeignKey(S => S.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many relationship for Class and Subject
            modelBuilder.Entity<Class>()
                .HasMany<Subject>(S => S.Subjects)
                .WithOne(C => C.Class)
                .HasForeignKey(S => S.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many relationship for Class and Division
            modelBuilder.Entity<Class>()
                .HasMany<Division>(D => D.Divisions)
                .WithOne(C => C.Class)
                .HasForeignKey(D => D.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many relationship for Division and Student
            modelBuilder.Entity<Division>()
                .HasMany<Student>(S => S.Students)
                .WithOne(D => D.Division)
                .HasForeignKey(S => S.DivisionID)
                .OnDelete(DeleteBehavior.Restrict);

            // one to many relationship for Teacher and Salary
            modelBuilder.Entity<Teacher>()
            .HasMany<Salary>(S => S.Salaries)
            .WithOne(T => T.Teacher)
            .HasForeignKey(S => S.TeacherID)
            .OnDelete(DeleteBehavior.Restrict);

            // one to many relationship for Guardian and Students
            modelBuilder.Entity<Guardian>()
                .HasMany<Student>(S => S.Students)
                .WithOne(G => G.Guardian)
                .HasForeignKey(S => S.GuardianID)
                .OnDelete(DeleteBehavior.Cascade);

            //composite Atribute for Student and Name
            modelBuilder.Entity<Student>()
            .OwnsOne(s => s.FullName);

            //composite Atribute for Guardian and Name
            modelBuilder.Entity<Guardian>()
            .OwnsOne(G => G.FullName);

            //composite Atribute for teacher and Name
            modelBuilder.Entity<Teacher>()
            .OwnsOne(T => T.FullName);

            //composite Atribute for Manager and Name
            modelBuilder.Entity<Manager>()
            .OwnsOne(M => M.FullName);

            //this is for Roles 
            modelBuilder.Entity<User>()
            .HasOne(u => u.Guardian)
            .WithOne(g => g.User)
            .HasForeignKey<Guardian>(g => g.UserID)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithOne(m => m.User)
                .HasForeignKey<Manager>(m => m.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Ternary relationship between Teachers, Students, and Subjects
            modelBuilder.Entity<TeacherSubjectStudent>()
                .HasOne<Teacher>(tss => tss.Teacher)
                .WithMany(t => t.TeacherSubjectStudents)
                .HasForeignKey(tss => tss.TeacherID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherSubjectStudent>()
                .HasOne<Student>(tss => tss.Student)
                .WithMany(s => s.TeacherSubjectStudents)
                .HasForeignKey(tss => tss.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherSubjectStudent>()
                .HasOne<Subject>(tss => tss.Subject)
                .WithMany(sub => sub.TeacherSubjectStudents)
                .HasForeignKey(tss => tss.SubjectID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
