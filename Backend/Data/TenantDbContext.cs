using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Data
{
    public class TenantDbContext : DbContext
    {
        private readonly TenantInfo _tenant;
        private static readonly SemaphoreSlim _fixSemaphore = new SemaphoreSlim(1, 1);
        private static readonly ConcurrentDictionary<string, bool> _fixedDatabases = new();

        public TenantDbContext(DbContextOptions<TenantDbContext> options, TenantInfo tenant) : base(options)
        {
            _tenant = tenant;
        }

        // Tenant tables (keep your school system DbSets here)
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
        public DbSet<GradeType> GradeTypes { get; set; }
        public DbSet<YearTermMonth> YearTermMonths { get; set; }
        public DbSet<StudentClassFees> StudentClassFees { get; set; }
        public DbSet<TermlyGrade> TermlyGrades { get; set; }
        public DbSet<AccountStudentGuardian> AccountStudentGuardians { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            // Only configure if connection string is available
            // If not available (admin endpoints), OnConfiguring won't configure it
            // and we'll throw when actually used (lazy evaluation)
            if (!string.IsNullOrWhiteSpace(_tenant.ConnectionString))
            {
                optionsBuilder.UseSqlServer(_tenant.ConnectionString, sql =>
                {
                    sql.CommandTimeout(180);
                });
            }
        }

        // Override Database property to check connection string when accessed
        public override Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database
        {
            get
            {
                EnsureConnectionString();
                return base.Database;
            }
        }

        private void EnsureConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_tenant.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string not resolved. For ADMIN users accessing School endpoints, " +
                    "the admin connection string should be set. For MANAGER users, ensure the TenantId claim " +
                    "is present in your JWT token and the tenant exists in the database.");
            }
        }

        /// <summary>
        /// Fixes existing tenant databases by removing FK constraints to AspNetUsers
        /// </summary>
        private async Task FixTenantDatabaseConstraintsAsync()
        {
            if (string.IsNullOrWhiteSpace(_tenant.ConnectionString))
                return;

            // Check if we've already fixed this database
            if (_fixedDatabases.ContainsKey(_tenant.ConnectionString))
                return;

            await _fixSemaphore.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_fixedDatabases.ContainsKey(_tenant.ConnectionString))
                    return;

                // Use a new connection to drop constraints (outside of any EF transaction)
                using var connection = new SqlConnection(_tenant.ConnectionString);
                await connection.OpenAsync();

                // First, get all FK constraints that reference AspNetUsers
                var getFkConstraintsSql = @"
                    SELECT 
                        fk.name AS ForeignKeyName,
                        OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SchemaName,
                        OBJECT_NAME(fk.parent_object_id) AS TableName
                    FROM sys.foreign_keys fk
                    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                    INNER JOIN sys.objects ref_obj ON fkc.referenced_object_id = ref_obj.object_id
                    WHERE ref_obj.name = 'AspNetUsers'
                      AND (OBJECT_NAME(fk.parent_object_id) IN ('Guardians', 'Students', 'Teachers', 'Managers')
                           OR fk.name LIKE 'FK_Guardians_AspNetUsers%'
                           OR fk.name LIKE 'FK_Students_AspNetUsers%'
                           OR fk.name LIKE 'FK_Teachers_AspNetUsers%'
                           OR fk.name LIKE 'FK_Managers_AspNetUsers%')";

                var constraintsToDrop = new List<(string SchemaName, string TableName, string ConstraintName)>();

                using (var command = new SqlCommand(getFkConstraintsSql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var schemaName = reader.GetString(1);
                        var tableName = reader.GetString(2);
                        var constraintName = reader.GetString(0);
                        constraintsToDrop.Add((schemaName, tableName, constraintName));
                    }
                }

                // Drop all found FK constraints
                foreach (var (schemaName, tableName, constraintName) in constraintsToDrop)
                {
                    try
                    {
                        var dropSql = $@"ALTER TABLE [{schemaName}].[{tableName}] DROP CONSTRAINT [{constraintName}];";
                        using var dropCommand = new SqlCommand(dropSql, connection);
                        await dropCommand.ExecuteNonQueryAsync();
                        System.Diagnostics.Debug.WriteLine($"Dropped FK constraint: {constraintName} from {schemaName}.{tableName}");
                    }
                    catch (Exception ex)
                    {
                        // Log but don't throw - continue with other constraints
                        System.Diagnostics.Debug.WriteLine($"Could not drop constraint {constraintName} from {schemaName}.{tableName}: {ex.Message}");
                    }
                }

                // Also try the known constraint names as a fallback
                var knownConstraints = new[]
                {
                    ("FK_Guardians_AspNetUsers_UserID", "Guardians"),
                    ("FK_Students_AspNetUsers_UserID", "Students"),
                    ("FK_Teachers_AspNetUsers_UserID", "Teachers"),
                    ("FK_Managers_AspNetUsers_UserID", "Managers")
                };

                foreach (var (constraintName, tableName) in knownConstraints)
                {
                    try
                    {
                        var sql = $@"
                            IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '{constraintName}')
                            BEGIN
                                ALTER TABLE [dbo].[{tableName}] DROP CONSTRAINT [{constraintName}];
                            END";

                        using var command = new SqlCommand(sql, connection);
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log but don't throw - continue with other constraints
                        System.Diagnostics.Debug.WriteLine($"Could not drop constraint {constraintName}: {ex.Message}");
                    }
                }

                // Mark this database as fixed
                _fixedDatabases.TryAdd(_tenant.ConnectionString, true);
            }
            catch (Exception ex)
            {
                // Log but don't throw - we'll try again on the next SaveChanges if needed
                System.Diagnostics.Debug.WriteLine($"Error fixing tenant database constraints: {ex.Message}");
            }
            finally
            {
                _fixSemaphore.Release();
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Proactively fix database constraints before attempting to save (if not already fixed)
            if (!string.IsNullOrWhiteSpace(_tenant.ConnectionString) && 
                !_fixedDatabases.ContainsKey(_tenant.ConnectionString))
            {
                await FixTenantDatabaseConstraintsAsync();
            }

            int retryCount = 0;
            const int maxRetries = 2; // Allow up to 2 retries

            while (retryCount <= maxRetries)
            {
                try
                {
                    return await base.SaveChangesAsync(cancellationToken);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    // Check if it's a foreign key constraint violation related to AspNetUsers
                    if (ex.InnerException is SqlException sqlEx && 
                        sqlEx.Number == 547 && 
                        (sqlEx.Message.Contains("FK_") || sqlEx.Message.Contains("AspNetUsers")))
                    {
                        if (retryCount < maxRetries)
                        {
                            // Foreign key constraint violation - try to fix the database again (in case it wasn't fixed)
                            // Remove from fixed databases so we can try to fix again
                            _fixedDatabases.TryRemove(_tenant.ConnectionString ?? "", out _);
                            await FixTenantDatabaseConstraintsAsync();
                            retryCount++;
                            // Continue loop to retry
                            continue;
                        }
                    }
                    // If not a FK constraint error or max retries reached, throw
                    throw;
                }
            }

            // Should never reach here, but just in case
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: remove Identity stuff from tenant DB
            // Cross-database FK to ApplicationUser cannot work, so ignore navs if they exist in your models.
            // Also remove foreign key constraints to AspNetUsers since it doesn't exist in tenant DB
            modelBuilder.Entity<Teacher>().Ignore(x => x.ApplicationUser);
            modelBuilder.Entity<Student>().Ignore(x => x.ApplicationUser);
            modelBuilder.Entity<Guardian>().Ignore(x => x.ApplicationUser);
            modelBuilder.Entity<Manager>().Ignore(x => x.ApplicationUser);
            
            // Explicitly configure UserID properties to have NO foreign key relationship
            // UserID is stored as a string but without FK constraint (users are in admin DB)
            modelBuilder.Entity<Guardian>()
                .Property(g => g.UserID)
                .IsRequired(false); // Make it optional so FK constraint won't be enforced
            
            modelBuilder.Entity<Student>()
                .Property(s => s.UserID)
                .IsRequired(false);
            
            modelBuilder.Entity<Teacher>()
                .Property(t => t.UserID)
                .IsRequired(false);
            
            modelBuilder.Entity<Manager>()
                .Property(m => m.UserID)
                .IsRequired(false);
            
            // Remove any unique indexes on UserID that might enforce FK relationships
            // These will be dropped when the FK constraints are dropped

            // ✅ Now paste MOST of your existing mappings here as-is
            // - Keep all keys / relationships / seeds (Month/Term/GradeTypes/TypeAccount) here
            // - REMOVE these from tenant:
            //   - IdentityRole seeding
            //   - ApplicationUser seeding
            //   - RefreshToken mapping
            //   - Tenant mapping (Tenants table)

            // Configure primary keys for entities
            modelBuilder.Entity<Student>()
                .HasKey(s => s.StudentID);
            
            modelBuilder.Entity<Accounts>()
                .HasKey(a => a.AccountID);
            
            modelBuilder.Entity<Fee>()
                .HasKey(a => a.FeeID);
            
            modelBuilder.Entity<Attachments>()
                .HasKey(a => a.AttachmentID);
            
            // Attachments -> Student
            modelBuilder.Entity<Attachments>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attachments)
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Attachments -> Voucher
            modelBuilder.Entity<Attachments>()
                .HasOne(a => a.Voucher)
                .WithMany(v => v.Attachments)
                .HasForeignKey(a => a.VoucherID)
                .OnDelete(DeleteBehavior.SetNull);

            
            modelBuilder.Entity<StudentClassFees>()
                .HasKey(a => a.StudentClassFeesID);
            
            modelBuilder.Entity<Vouchers>()
                .HasKey(v => v.VoucherID);
            
            modelBuilder.Entity<Term>()
                .HasKey(t => t.TermID);
            
            modelBuilder.Entity<School>()
                .HasKey(s => s.SchoolID);
            
            modelBuilder.Entity<Class>()
                .HasKey(c => c.ClassID);
            
            modelBuilder.Entity<Division>()
                .HasKey(d => d.DivisionID);
            
            modelBuilder.Entity<Subject>()
                .HasKey(s => s.SubjectID);
            
            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.TeacherID);
            
            modelBuilder.Entity<Manager>()
                .HasKey(m => m.ManagerID);
            
            modelBuilder.Entity<Guardian>()
                .HasKey(g => g.GuardianID);
            
            modelBuilder.Entity<Year>()
                .HasKey(y => y.YearID);
            
            modelBuilder.Entity<Stage>()
                .HasKey(s => s.StageID);
            
            modelBuilder.Entity<Month>()
                .HasKey(m => m.MonthID);
            
            modelBuilder.Entity<FeeClass>()
                .HasKey(e => e.FeeClassID);
            
            modelBuilder.Entity<TypeAccount>()
                .HasKey(t => t.TypeAccountID);
            
            modelBuilder.Entity<GradeType>()
                .HasKey(g => g.GradeTypeID);
            
            modelBuilder.Entity<Curriculum>()
                .HasKey(c => new { c.SubjectID, c.ClassID });
            
            modelBuilder.Entity<CoursePlan>()
                .HasKey(c => new { c.YearID, c.TeacherID, c.ClassID, c.DivisionID, c.SubjectID });
            
            modelBuilder.Entity<MonthlyGrade>()
                .HasKey(mg => new { mg.StudentID, mg.YearID, mg.SubjectID, mg.MonthID, mg.GradeTypeID, mg.ClassID, mg.TermID });
            
            modelBuilder.Entity<YearTermMonth>()
                .HasKey(ytm => new { ytm.YearID, ytm.TermID, ytm.MonthID });
            
            modelBuilder.Entity<TermlyGrade>()
                .HasKey(t => t.TermlyGradeID);
            
            modelBuilder.Entity<AccountStudentGuardian>()
                .HasKey(a => a.AccountStudentGuardianID);
            
            modelBuilder.Entity<Salary>()
                .HasKey(s => s.SalaryID);

            // Configure owned entities (composite types)
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.FullName);
            
            // Configure FullNameAlis as required navigation to always create an instance
            // This fixes the warning about optional dependent with table sharing
            // All properties remain nullable, but EF will always create the instance
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.FullNameAlis);
            
            modelBuilder.Entity<Student>()
                .Navigation(s => s.FullNameAlis)
                .IsRequired();
            
            modelBuilder.Entity<Teacher>()
                .OwnsOne(T => T.FullName);
            
            modelBuilder.Entity<Manager>()
                .OwnsOne(M => M.FullName);

            // Configure decimal precision
            modelBuilder.Entity<Accounts>()
                .Property(v => v.OpenBalance)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<AccountStudentGuardian>()
                .Property(v => v.Amount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Vouchers>()
                .Property(v => v.Receipt)
                .HasColumnType("decimal(18,2)");
            
            // Configure decimal precision for grade-related entities
            modelBuilder.Entity<GradeType>()
                .Property(g => g.MaxGrade)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<MonthlyGrade>()
                .Property(mg => mg.Grade)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<StudentClassFees>()
                .Property(scf => scf.AmountDiscount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<TermlyGrade>()
                .Property(tg => tg.Grade)
                .HasColumnType("decimal(18,2)");

            // Configure AccountStudentGuardian relationships
            modelBuilder.Entity<AccountStudentGuardian>()
                .HasOne(ASG => ASG.Accounts)
                .WithMany(a => a.AccountStudentGuardians)
                .HasForeignKey(ASG => ASG.AccountID)
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

            // Configure Accounts relationship with TypeAccount
            modelBuilder.Entity<Accounts>()
                .HasOne(a => a.TypeAccount)
                .WithMany(t => t.Accounts)
                .HasForeignKey(a => a.TypeAccountID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Vouchers relationship
            modelBuilder.Entity<Vouchers>()
                .HasOne(v => v.AccountStudentGuardians)
                .WithMany(ASG => ASG.Vouchers)
                .HasForeignKey(v => v.AccountStudentGuardianID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure School -> Years relationship (one to many)
            modelBuilder.Entity<School>()
                .HasMany<Year>(s => s.Years)
                .WithOne(Y => Y.School)
                .HasForeignKey(Y => Y.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data for tenant databases
            modelBuilder.Entity<TypeAccount>()
                .HasData(
                    new TypeAccount { TypeAccountID = 1, TypeAccountName = "Guardain" },
                    new TypeAccount { TypeAccountID = 2, TypeAccountName = "School" },
                    new TypeAccount { TypeAccountID = 3, TypeAccountName = "Branches" },
                    new TypeAccount { TypeAccountID = 4, TypeAccountName = "Funds" },
                    new TypeAccount { TypeAccountID = 5, TypeAccountName = "Employees" },
                    new TypeAccount { TypeAccountID = 6, TypeAccountName = "Banks" }
                );

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
        }
    }
}
