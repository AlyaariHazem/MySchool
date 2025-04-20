using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Attachments> Attachments { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Salary> Salarys { get; set; }
        public DbSet<CoursePlan> CoursePlans { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Year> Years { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Curriculum> Curriculums { get; set; }
        public DbSet<Month> Months { get; set; }
        public DbSet<FeeClass> FeeClass { get; set; }
        public DbSet<Vouchers> Vouchers { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<MonthlyGrade> MonthlyGrades { get; set; }
        public DbSet<TypeAccount> TypeAccounts { get; set; }
        // public DbSet<SubjectStudent> SubjectStudents { get; set; }
        public DbSet<GradeType> GradeTypes { get; set; }
        public DbSet<YearTermMonth> YearTermMonths { get; set; }
        public DbSet<StudentClassFees> StudentClassFees { get; set; }
        // public DbSet<TeacherStudent> TeacherStudents { get; set; }
        public DbSet<TermlyGrade> TermlyGrades { get; set; }
        public DbSet<AccountStudentGuardian> AccountStudentGuardians { get; set; }
        // public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.TenantId);
            });

            base.OnModelCreating(modelBuilder); // Call the base method

            // modelBuilder.Entity<TeacherStudent>().HasKey(TS => new { TS.StudentID, TS.TeacherID });
            // modelBuilder.Entity<SubjectStudent>().HasKey(SS => new { SS.SubjectID, SS.StudentID });
            modelBuilder.Entity<Curriculum>().HasKey(SS => new { SS.SubjectID, SS.ClassID });
            modelBuilder.Entity<CoursePlan>().HasKey(c => new { c.YearID, c.TeacherID, c.ClassID, c.DivisionID, c.SubjectID });
            modelBuilder.Entity<MonthlyGrade>().HasKey(mg => new { mg.StudentID, mg.YearID, mg.SubjectID, mg.MonthID, mg.GradeTypeID, mg.ClassID, mg.TermID });

            modelBuilder.Entity<YearTermMonth>().HasKey(ytm => new { ytm.YearID, ytm.TermID, ytm.MonthID });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentID); // Primary key
                entity.Property(s => s.StudentID)
                    .ValueGeneratedNever(); // Disables auto-increment
            });
            modelBuilder.Entity<Month>(entity =>
            {
                entity.HasKey(m => m.MonthID); // Primary key
                entity.Property(m => m.MonthID)
                    .ValueGeneratedNever();
            });
            modelBuilder.Entity<TermlyGrade>(entity =>
            {
                entity.HasKey(t => t.TermlyGradeID); // Primary key
                entity.HasIndex(t => new { t.StudentID, t.TermID, t.SubjectID, t.ClassID })
                      .IsUnique();
            });
            modelBuilder.Entity<FeeClass>(entity =>
            {
                entity.HasKey(e => e.FeeClassID); // Single-column primary key
                entity.Property(e => e.FeeClassID).ValueGeneratedOnAdd();
                entity.Property(fc => fc.ClassID).IsRequired();
                entity.Property(fc => fc.FeeID).IsRequired();
            });


            modelBuilder.Entity<Accounts>()
                .HasKey(a => a.AccountID);

            modelBuilder.Entity<Fee>()
                .HasKey(a => a.FeeID);

            modelBuilder.Entity<Attachments>()
                .HasKey(a => a.AttachmentID);

            modelBuilder.Entity<StudentClassFees>()
                .HasKey(a => a.StudentClassFeesID);

            modelBuilder.Entity<Vouchers>()
                .HasKey(v => v.VoucherID);

            // modelBuilder.Entity<TeacherSubjectStudent>()
            //     .HasKey(tss => new { tss.TeacherID, tss.StudentID, tss.SubjectID });

            modelBuilder.Entity<Term>()
                .HasKey(t => t.TermID);

            modelBuilder.Entity<School>()
                .HasOne<Manager>(m => m.Manager)
                .WithOne(s => s.School)
                .HasForeignKey<Manager>(m => m.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tenant>()
                .HasOne<Manager>(m => m.Manager)
                .WithOne(s => s.Tenant)
                .HasForeignKey<Manager>(m => m.TenantID)
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
            // one to many for Year and Class
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Year)
                .WithMany(p => p.Classes)
                .HasForeignKey(c => c.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            // many to many relationship for Subjects and Students
            modelBuilder.Entity<Curriculum>()
                .HasOne(C => C.Subject)
                .WithMany(S => S.Curriculums)
                .HasForeignKey(C => C.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Curriculum>()
                .HasOne(C => C.Class)
                .WithMany(c => c.Curriculums)
                .HasForeignKey(C => C.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            // many to many relationship for Classes and Fees
            modelBuilder.Entity<FeeClass>()
                .HasOne(fc => fc.Class)
                .WithMany(c => c.FeeClasses)
                .HasForeignKey(fc => fc.ClassID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FeeClass>()
                .HasOne(fc => fc.Fee)
                .WithMany(f => f.FeeClasses)
                .HasForeignKey(fc => fc.FeeID)
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

            modelBuilder.Entity<Student>()
            .HasOne(s => s.Guardian)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GuardianID)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
            .Property(s => s.GuardianID)
            .HasDefaultValue(1012); // Default GuardianID value

            //composite Atribute for Student and Name
            modelBuilder.Entity<Student>()
            .OwnsOne(s => s.FullName);

            //composite Atribute for Student and NameEng
            modelBuilder.Entity<Student>()
            .OwnsOne(s => s.FullNameAlis);

            //composite Atribute for teacher and Name
            modelBuilder.Entity<Teacher>()
            .OwnsOne(T => T.FullName);

            //composite Atribute for Manager and Name
            modelBuilder.Entity<Manager>()
            .OwnsOne(M => M.FullName);

            // Month -> Term
            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Month)
                .WithMany(sm => sm.MonthlyGrades)
                .HasForeignKey(mg => mg.MonthID)
                .OnDelete(DeleteBehavior.Restrict);

            // Class
            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Class)
                .WithMany(c => c.MonthlyGrades)
                .HasForeignKey(mg => mg.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TermlyGrade>()
                .HasOne(mg => mg.Student)
                .WithMany(s => s.TermlyGrades)
                .HasForeignKey(mg => mg.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TermlyGrade>()
                .HasOne(mg => mg.Class)
                .WithMany(s => s.TermlyGrades)
                .HasForeignKey(mg => mg.ClassID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TermlyGrade>()
                .HasOne(mg => mg.Subject)
                .WithMany(s => s.TermlyGrades)
                .HasForeignKey(mg => mg.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TermlyGrade>()
                .HasOne(mg => mg.Term)
                .WithMany(s => s.TermlyGrades)
                .HasForeignKey(mg => mg.TermID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TermlyGrade>()
                .HasOne(mg => mg.Year)
                .WithMany(s => s.TermlyGrades)
                .HasForeignKey(mg => mg.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            // Etc. (Student, Subject, GradeType)

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Student)
                .WithMany(s => s.MonthlyGrades)
                .HasForeignKey(mg => mg.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Subject)
                .WithMany(sbj => sbj.MonthlyGrades)
                .HasForeignKey(mg => mg.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Year)
                .WithMany(y => y.MonthlyGrades)
                .HasForeignKey(mg => mg.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Term)
                .WithMany(y => y.MonthlyGrades)
                .HasForeignKey(mg => mg.TermID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.GradeType)
                .WithMany(gt => gt.MonthlyGrades)
                .HasForeignKey(mg => mg.GradeTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachments>()
               .HasOne(a => a.Student)
               .WithMany(s => s.Attachments)
               .HasForeignKey(a => a.StudentID)
               .OnDelete(DeleteBehavior.Cascade);// Optional relationship

            modelBuilder.Entity<Attachments>()
               .HasOne(a => a.Vouchers)
               .WithMany(v => v.Attachments)
               .HasForeignKey(a => a.VoucherID)
               .OnDelete(DeleteBehavior.Cascade);// Optional relationship


            // One-to-One: ApplicationUser ↔ Teacher
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.ApplicationUser)
                .HasForeignKey<Teacher>(t => t.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: ApplicationUser ↔ Student
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Student)
                .WithOne(s => s.ApplicationUser)
                .HasForeignKey<Student>(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: ApplicationUser ↔ Guardian
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Guardian)
                .WithOne(g => g.ApplicationUser)
                .HasForeignKey<Guardian>(g => g.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: ApplicationUser ↔ Manager
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Manager)
                .WithOne(m => m.ApplicationUser)
                .HasForeignKey<Manager>(m => m.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Ternary relationship between Guardian, Students,Vouchers, and Accounts
            modelBuilder.Entity<AccountStudentGuardian>()
                .HasOne(ASG => ASG.Accounts)
                .WithMany(a => a.AccountStudentGuardians)
                .HasForeignKey(ASG => ASG.AccountID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vouchers>()
                .HasOne(v => v.AccountStudentGuardians)
                .WithMany(ASG => ASG.Vouchers)
                .HasForeignKey(v => v.AccountStudentGuardianID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountStudentGuardian>()
               .HasOne(ASG => ASG.Guardian)
               .WithMany(a => a.AccountStudentGuardians)
               .HasForeignKey(ASG => ASG.GuardianID)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountStudentGuardian>()
               .HasOne(ASG => ASG.Student)
               .WithMany(a => a.AccountStudentGuardians)
               .HasForeignKey(ASG => ASG.StudentID)
               .OnDelete(DeleteBehavior.Restrict);

            // FeeClass to StudentClassFees
            modelBuilder.Entity<StudentClassFees>()
                .HasOne(scf => scf.FeeClass)
                .WithMany(fc => fc.StudentClassFees)
                .HasForeignKey(scf => scf.FeeClassID)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TypeAccount>()
                .HasMany(t => t.Accounts)
                .WithOne(a => a.TypeAccount)
                .HasForeignKey(a => a.TypeAccountID)
                .OnDelete(DeleteBehavior.Cascade);

            // Student to StudentClassFees
            modelBuilder.Entity<StudentClassFees>()
                .HasOne(scf => scf.Student)
                .WithMany(s => s.StudentClassFees)
                .HasForeignKey(scf => scf.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            //CoursePlan relationships
            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Year)
                .WithMany(y => y.CoursePlans)  // or whatever you call the navigation
                .HasForeignKey(p => p.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Term)
                .WithMany(t => t.CoursePlans)
                .HasForeignKey(p => p.TermID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Subject)
                .WithMany(s => s.CoursePlans)
                .HasForeignKey(p => p.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Teacher)
                .WithMany(tch => tch.CoursePlans)
                .HasForeignKey(p => p.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Class)
                .WithMany(c => c.CoursePlans)
                .HasForeignKey(p => p.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(p => p.Division)
                .WithMany(c => c.CoursePlans)
                .HasForeignKey(p => p.DivisionID)
                .OnDelete(DeleteBehavior.Restrict);
            //YearTermMonth
            modelBuilder.Entity<YearTermMonth>()
                .HasOne(ytm => ytm.Year)
                .WithMany(y => y.YearTermMonths)
                .HasForeignKey(ytm => ytm.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YearTermMonth>()
                .HasOne(ytm => ytm.Month)
                .WithMany(m => m.YearTermMonths)
                .HasForeignKey(ytm => ytm.MonthID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YearTermMonth>()
                .HasOne(ytm => ytm.Term)
                .WithMany(t => t.YearTermMonths)
                .HasForeignKey(ytm => ytm.TermID)
                .OnDelete(DeleteBehavior.Restrict);


            // Composite Unique Index
            modelBuilder.Entity<MonthlyGrade>()
                .HasIndex(mg => new
                {
                    mg.StudentID,
                    mg.SubjectID,
                    mg.MonthID,
                    mg.GradeTypeID
                })
                .IsUnique();

            modelBuilder.Entity<Vouchers>()
                .Property(v => v.Receipt)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Accounts>()
                .Property(v => v.OpenBalance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AccountStudentGuardian>()
                .Property(v => v.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<TypeAccount>()
                .HasData(new TypeAccount { TypeAccountID = 1, TypeAccountName = "Guardain" },
                new TypeAccount { TypeAccountID = 2, TypeAccountName = "School" },
                new TypeAccount { TypeAccountID = 3, TypeAccountName = "Branches" },
                new TypeAccount { TypeAccountID = 4, TypeAccountName = "Funds" },
                new TypeAccount { TypeAccountID = 5, TypeAccountName = "Employees" },
                new TypeAccount { TypeAccountID = 6, TypeAccountName = "Banks" });

            modelBuilder.Entity<GradeType>().HasData(
                new GradeType { GradeTypeID = 1, Name = "Assignments", MaxGrade = 20, IsActive = true },
                new GradeType { GradeTypeID = 2, Name = "Attendance", MaxGrade = 20, IsActive = true },
                new GradeType { GradeTypeID = 3, Name = "Participation", MaxGrade = 10, IsActive = true },
                new GradeType { GradeTypeID = 4, Name = "Oral", MaxGrade = 10, IsActive = true },
                new GradeType { GradeTypeID = 5, Name = "Exam", MaxGrade = 40, IsActive = true },
                new GradeType { GradeTypeID = 6, Name = "work", MaxGrade = 20, IsActive = false },
                new GradeType { GradeTypeID = 7, Name = "lab", MaxGrade = 30, IsActive = false },
                new GradeType { GradeTypeID = 8, Name = "skills", MaxGrade = 20, IsActive = false }
            );

            modelBuilder.Entity<Term>().HasData(
                new Term { TermID = 1, Name = "الأول" },
                new Term { TermID = 2, Name = "الثاني" }
            );
            modelBuilder.Entity<Month>().HasData(
                new Month { MonthID = 1, Name = "يناير" },
                new Month { MonthID = 2, Name = "فبراير" },
                new Month { MonthID = 3, Name = "مارس" },
                new Month { MonthID = 4, Name = "أبريل" },
                new Month { MonthID = 5, Name = "مايو" },
                new Month { MonthID = 6, Name = "يونيو" },
                new Month { MonthID = 7, Name = "يوليو" },
                new Month { MonthID = 8, Name = "أغسطس" },
                new Month { MonthID = 9, Name = "سبتمبر" },
                new Month { MonthID = 10, Name = "أكتوبر" },
                new Month { MonthID = 11, Name = "نوفمبر" },
                new Month { MonthID = 12, Name = "ديسمبر" }
            );
            // Seed data for roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "ADMIN",
                    NormalizedName = "ADMIN"
                }
            );
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "2",
                    Name = "MANAGER",
                    NormalizedName = "MANAGER"
                }
            );
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "3",
                    Name = "STUDENT",
                    NormalizedName = "STUDENT"
                }
            );
            // Seed data for roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "4",
                    Name = "TEACHER",
                    NormalizedName = "TEACHER"
                }
            );
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "5",
                    Name = "GUARDIAN",
                    NormalizedName = "GUARDIAN"
                }
            );
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var hashedPassword = passwordHasher.HashPassword(null, "ADMIN");

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                    UserName = "ADMIN",
                    NormalizedUserName = "ADMIN",
                    Email = "ADMIN@GMAIL.COM",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    PasswordHash = hashedPassword, // Store the hashed password here
                    EmailConfirmed = true,
                    UserType = "ADMIN" // You can assign a specific role or user type if needed
                });

        }
    }
}