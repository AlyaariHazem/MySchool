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
        public DbSet<SchoolStaff> SchoolStaff { get; set; }
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
        public DbSet<ReportTemplate> ReportTemplates { get; set; }
        public DbSet<WeeklySchedule> WeeklySchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<ExamType> ExamTypes { get; set; }
        public DbSet<ScheduledExam> ScheduledExams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<HomeworkTask> HomeworkTasks { get; set; }
        public DbSet<HomeworkTaskLink> HomeworkTaskLinks { get; set; }
        public DbSet<HomeworkSubmission> HomeworkSubmissions { get; set; }
        public DbSet<HomeworkSubmissionFile> HomeworkSubmissionFiles { get; set; }
        public DbSet<EmployeeYearAssignment> EmployeeYearAssignments { get; set; }
        public DbSet<EmployeeJobType> EmployeeJobTypes { get; set; }
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<EmployeeQualification> EmployeeQualifications { get; set; }
        public DbSet<EmployeeSpecialization> EmployeeSpecializations { get; set; }
        public DbSet<EmployeeHistory> EmployeeHistories { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
        public DbSet<EmployeePerformanceSummary> EmployeePerformanceSummaries { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Interview> RecruitmentInterviews { get; set; }
        public DbSet<CandidateEvaluation> CandidateEvaluations { get; set; }
        public DbSet<HiringDecision> HiringDecisions { get; set; }

        public DbSet<DailyEvaluationTemplate> DailyEvaluationTemplates { get; set; }
        public DbSet<DailyEvaluationCriteria> DailyEvaluationCriteria { get; set; }
        public DbSet<DailyEvaluation> DailyEvaluations { get; set; }
        public DbSet<DailyEvaluationItem> DailyEvaluationItems { get; set; }
        public DbSet<EvaluationLock> EvaluationLocks { get; set; }
        public DbSet<EvaluationOverrideLog> EvaluationOverrideLogs { get; set; }

        public DbSet<SupervisorVisit> SupervisorVisits { get; set; }
        public DbSet<VisitObservation> VisitObservations { get; set; }
        public DbSet<VisitRecommendation> VisitRecommendations { get; set; }
        public DbSet<RecommendationFollowUp> RecommendationFollowUps { get; set; }

        public DbSet<TeacherFeedbackCycle> TeacherFeedbackCycles { get; set; }
        public DbSet<FeedbackQuestion> FeedbackQuestions { get; set; }
        public DbSet<StudentFeedback> StudentFeedbacks { get; set; }
        public DbSet<ParentFeedback> ParentFeedbacks { get; set; }
        public DbSet<FeedbackSummary> FeedbackSummaries { get; set; }

        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<EmployeeRequest> EmployeeRequests { get; set; }
        public DbSet<RequestApprovalStep> RequestApprovalSteps { get; set; }
        public DbSet<RequestExecution> RequestExecutions { get; set; }
        public DbSet<RequestDailySummary> RequestDailySummaries { get; set; }

        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<AchievementRequest> AchievementRequests { get; set; }
        public DbSet<AchievementApproval> AchievementApprovals { get; set; }
        public DbSet<AchievementAttachment> AchievementAttachments { get; set; }
        public DbSet<AchievementPointsLedger> AchievementPointsLedgers { get; set; }

        public DbSet<ViolationType> ViolationTypes { get; set; }
        public DbSet<Violation> Violations { get; set; }
        public DbSet<ViolationResponse> ViolationResponses { get; set; }
        public DbSet<ViolationAction> ViolationActions { get; set; }
        public DbSet<ViolationEscalationHistory> ViolationEscalationHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            // Only configure if connection string is available
            // If not available (admin endpoints), OnConfiguring won't configure it
            // and we'll throw when actually used (lazy evaluation)
            if (!string.IsNullOrWhiteSpace(_tenant.ConnectionString))
                optionsBuilder.UseTenantSqlServer(_tenant.ConnectionString);
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
            modelBuilder.Entity<Manager>().Ignore(x => x.Tenant);
            
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
            modelBuilder.Entity<Student>()
                .Property(s => s.StudentID)
                .ValueGeneratedNever();

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

            // SQL Server: CASCADE on Student plus Class→FeeClass→StudentClassFees creates multiple cascade paths.
            modelBuilder.Entity<StudentClassFees>()
                .HasOne(scf => scf.Student)
                .WithMany(s => s.StudentClassFees)
                .HasForeignKey(scf => scf.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            
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

            // One school, many managers (do not use School.Manager one-to-one — unique IX_Managers_SchoolID).
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.School)
                .WithMany(s => s.Managers)
                .HasForeignKey(m => m.SchoolID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolStaff>()
                .HasKey(s => s.SchoolStaffID);

            modelBuilder.Entity<SchoolStaff>()
                .Property(s => s.UserID)
                .IsRequired(false);

            modelBuilder.Entity<SchoolStaff>()
                .HasOne(s => s.School)
                .WithMany()
                .HasForeignKey(s => s.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            
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
                .HasKey(c => new { c.YearID, c.TeacherID, c.ClassID, c.DivisionID, c.SubjectID, c.TermID });

            // SQL Server: CASCADE on both Year and Class (and Division) creates multiple cascade paths to CoursePlans.
            modelBuilder.Entity<CoursePlan>()
                .HasOne(cp => cp.Year)
                .WithMany(y => y.CoursePlans)
                .HasForeignKey(cp => cp.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePlan>()
                .HasOne(cp => cp.Division)
                .WithMany(d => d.CoursePlans)
                .HasForeignKey(cp => cp.DivisionID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<MonthlyGrade>()
                .HasKey(mg => new { mg.StudentID, mg.YearID, mg.SubjectID, mg.MonthID, mg.GradeTypeID, mg.ClassID, mg.TermID });

            // SQL Server: CASCADE on Class (or Year) plus Student→…→Class chains creates multiple cascade paths.
            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Class)
                .WithMany(c => c.MonthlyGrades)
                .HasForeignKey(mg => mg.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyGrade>()
                .HasOne(mg => mg.Year)
                .WithMany(y => y.MonthlyGrades)
                .HasForeignKey(mg => mg.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<YearTermMonth>()
                .HasKey(ytm => new { ytm.YearID, ytm.TermID, ytm.MonthID });
            
            modelBuilder.Entity<TermlyGrade>()
                .HasKey(t => t.TermlyGradeID);

            modelBuilder.Entity<TermlyGrade>()
                .HasOne(tg => tg.Class)
                .WithMany(c => c.TermlyGrades)
                .HasForeignKey(tg => tg.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TermlyGrade>()
                .HasOne(tg => tg.Year)
                .WithMany(y => y.TermlyGrades)
                .HasForeignKey(tg => tg.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<AccountStudentGuardian>()
                .HasKey(a => a.AccountStudentGuardianID);
            
            modelBuilder.Entity<Salary>()
                .HasKey(s => s.SalaryID);

            modelBuilder.Entity<ReportTemplate>()
                .HasKey(rt => rt.Id);
            
            modelBuilder.Entity<WeeklySchedule>()
                .HasKey(ws => ws.WeeklyScheduleID);

            // SQL Server: CASCADE on both Class and Year would create multiple cascade paths (Year→Stages→Classes→…).
            modelBuilder.Entity<WeeklySchedule>()
                .HasOne(ws => ws.Year)
                .WithMany()
                .HasForeignKey(ws => ws.YearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasKey(a => a.AttendanceId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Class)
                .WithMany()
                .HasForeignKey(a => a.ClassID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentID, a.ClassID, a.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<NotificationMessage>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<NotificationMessage>()
                .Property(n => n.Title)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<NotificationMessage>()
                .Property(n => n.Body)
                .IsRequired();

            modelBuilder.Entity<NotificationMessage>()
                .Property(n => n.SentByUserId)
                .IsRequired();

            modelBuilder.Entity<NotificationMessage>()
                .Property(n => n.TargetKind)
                .HasConversion<byte>();

            modelBuilder.Entity<NotificationMessage>()
                .Property(n => n.RequestedChannels)
                .HasConversion<int>();

            modelBuilder.Entity<NotificationDelivery>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<NotificationDelivery>()
                .Property(d => d.RecipientUserId)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<NotificationDelivery>()
                .Property(d => d.Channel)
                .HasConversion<byte>();

            modelBuilder.Entity<NotificationDelivery>()
                .HasOne(d => d.NotificationMessage)
                .WithMany(m => m.Deliveries)
                .HasForeignKey(d => d.NotificationMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationDelivery>()
                .HasIndex(d => d.RecipientUserId);

            modelBuilder.Entity<NotificationDelivery>()
                .HasIndex(d => new { d.RecipientUserId, d.ReadAtUtc });

            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.AuditLogId);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Category)
                .HasMaxLength(128)
                .IsRequired();

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.ActorUserId)
                .HasMaxLength(450);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.ActorDisplayName)
                .HasMaxLength(512);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.CorrelationId)
                .HasMaxLength(128);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.CreatedAtUtc);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.Category, a.CreatedAtUtc });

            modelBuilder.Entity<ExamSession>()
                .HasKey(e => e.ExamSessionID);
            modelBuilder.Entity<ExamSession>()
                .Property(e => e.ExamSessionID)
                .UseIdentityColumn();
            modelBuilder.Entity<ExamSession>()
                .HasOne(es => es.Year)
                .WithMany()
                .HasForeignKey(es => es.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExamSession>()
                .HasOne(es => es.Term)
                .WithMany()
                .HasForeignKey(es => es.TermID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamType>()
                .HasKey(e => e.ExamTypeID);
            modelBuilder.Entity<ExamType>()
                .Property(e => e.ExamTypeID)
                .UseIdentityColumn();

            modelBuilder.Entity<ScheduledExam>()
                .HasKey(e => e.ScheduledExamID);
            modelBuilder.Entity<ScheduledExam>()
                .Property(e => e.ScheduledExamID)
                .UseIdentityColumn();
            modelBuilder.Entity<ScheduledExam>()
                .Property(e => e.TotalMarks)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ScheduledExam>()
                .Property(e => e.PassingMarks)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.ExamSession)
                .WithMany(s => s.ScheduledExams)
                .HasForeignKey(se => se.ExamSessionID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.ExamType)
                .WithMany(t => t.ScheduledExams)
                .HasForeignKey(se => se.ExamTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Year)
                .WithMany()
                .HasForeignKey(se => se.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Term)
                .WithMany()
                .HasForeignKey(se => se.TermID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Class)
                .WithMany()
                .HasForeignKey(se => se.ClassID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Division)
                .WithMany()
                .HasForeignKey(se => se.DivisionID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Subject)
                .WithMany()
                .HasForeignKey(se => se.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasOne(se => se.Teacher)
                .WithMany()
                .HasForeignKey(se => se.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduledExam>()
                .HasIndex(se => new { se.YearID, se.TermID, se.ClassID, se.DivisionID });
            modelBuilder.Entity<ScheduledExam>()
                .HasIndex(se => se.TeacherID);
            modelBuilder.Entity<ScheduledExam>()
                .HasIndex(se => se.ExamDate);

            modelBuilder.Entity<ExamResult>()
                .HasKey(r => r.ExamResultID);
            modelBuilder.Entity<ExamResult>()
                .Property(r => r.ExamResultID)
                .UseIdentityColumn();
            modelBuilder.Entity<ExamResult>()
                .Property(r => r.Score)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ExamResult>()
                .HasOne(r => r.ScheduledExam)
                .WithMany(se => se.ExamResults)
                .HasForeignKey(r => r.ScheduledExamID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ExamResult>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExamResult>()
                .HasIndex(r => new { r.ScheduledExamID, r.StudentID })
                .IsUnique();

            modelBuilder.Entity<HomeworkTask>()
                .HasKey(t => t.HomeworkTaskID);
            modelBuilder.Entity<HomeworkTask>()
                .Property(t => t.HomeworkTaskID)
                .UseIdentityColumn();
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Teacher)
                .WithMany()
                .HasForeignKey(t => t.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Year)
                .WithMany()
                .HasForeignKey(t => t.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Term)
                .WithMany()
                .HasForeignKey(t => t.TermID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Class)
                .WithMany()
                .HasForeignKey(t => t.ClassID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Division)
                .WithMany()
                .HasForeignKey(t => t.DivisionID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasOne(t => t.Subject)
                .WithMany()
                .HasForeignKey(t => t.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkTask>()
                .HasIndex(t => new { t.YearID, t.TermID, t.ClassID, t.DivisionID });
            modelBuilder.Entity<HomeworkTask>()
                .HasIndex(t => t.TeacherID);
            modelBuilder.Entity<HomeworkTask>()
                .HasIndex(t => t.DueDateUtc);

            modelBuilder.Entity<HomeworkTaskLink>()
                .HasKey(l => l.HomeworkTaskLinkID);
            modelBuilder.Entity<HomeworkTaskLink>()
                .Property(l => l.HomeworkTaskLinkID)
                .UseIdentityColumn();
            modelBuilder.Entity<HomeworkTaskLink>()
                .HasOne(l => l.HomeworkTask)
                .WithMany(t => t.Links)
                .HasForeignKey(l => l.HomeworkTaskID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HomeworkSubmission>()
                .HasKey(s => s.HomeworkSubmissionID);
            modelBuilder.Entity<HomeworkSubmission>()
                .Property(s => s.HomeworkSubmissionID)
                .UseIdentityColumn();
            modelBuilder.Entity<HomeworkSubmission>()
                .Property(s => s.Status)
                .HasConversion<byte>();
            modelBuilder.Entity<HomeworkSubmission>()
                .Property(s => s.Score)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<HomeworkSubmission>()
                .HasOne(s => s.HomeworkTask)
                .WithMany(t => t.Submissions)
                .HasForeignKey(s => s.HomeworkTaskID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<HomeworkSubmission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HomeworkSubmission>()
                .HasIndex(s => new { s.HomeworkTaskID, s.StudentID })
                .IsUnique();
            modelBuilder.Entity<HomeworkSubmission>()
                .HasIndex(s => s.StudentID);

            modelBuilder.Entity<HomeworkSubmissionFile>()
                .HasKey(f => f.HomeworkSubmissionFileID);
            modelBuilder.Entity<HomeworkSubmissionFile>()
                .Property(f => f.HomeworkSubmissionFileID)
                .UseIdentityColumn();
            modelBuilder.Entity<HomeworkSubmissionFile>()
                .HasOne(f => f.HomeworkSubmission)
                .WithMany(s => s.Files)
                .HasForeignKey(f => f.HomeworkSubmissionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeYearAssignment>()
                .HasKey(e => e.AssignmentID);
            modelBuilder.Entity<EmployeeYearAssignment>()
                .Property(e => e.AssignmentID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeYearAssignment>()
                .HasOne(e => e.Year)
                .WithMany()
                .HasForeignKey(e => e.YearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeYearAssignment>()
                .HasIndex(e => new { e.YearID, e.EmployeeRole, e.EmployeeEntityID })
                .IsUnique();
            modelBuilder.Entity<EmployeeYearAssignment>()
                .Property(e => e.EmployeeRole)
                .HasMaxLength(32);
            modelBuilder.Entity<EmployeeYearAssignment>()
                .Property(e => e.AssignmentStatus)
                .HasMaxLength(32);

            // --- HR: Employee profiles (School Performance Analysis foundation) ---
            modelBuilder.Entity<EmployeeJobType>()
                .HasKey(j => j.EmployeeJobTypeID);
            modelBuilder.Entity<EmployeeJobType>()
                .Property(j => j.EmployeeJobTypeID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeJobType>()
                .HasIndex(j => j.Code)
                .IsUnique();
            modelBuilder.Entity<EmployeeJobType>().HasData(
                new EmployeeJobType { EmployeeJobTypeID = 1, Code = "TEACHER", Name = "Teacher", NameAr = "معلم", SortOrder = 1, IsActive = true },
                new EmployeeJobType { EmployeeJobTypeID = 2, Code = "MANAGER", Name = "Manager", NameAr = "مدير", SortOrder = 2, IsActive = true },
                new EmployeeJobType { EmployeeJobTypeID = 3, Code = "SCHOOL_STAFF", Name = "School staff", NameAr = "موظف مدرسة", SortOrder = 3, IsActive = true },
                new EmployeeJobType { EmployeeJobTypeID = 4, Code = "ADMINISTRATOR", Name = "Administrator", NameAr = "إداري", SortOrder = 4, IsActive = true },
                new EmployeeJobType { EmployeeJobTypeID = 5, Code = "SUPPORT", Name = "Support", NameAr = "دعم", SortOrder = 5, IsActive = true },
                new EmployeeJobType { EmployeeJobTypeID = 6, Code = "OTHER", Name = "Other", NameAr = "أخرى", SortOrder = 99, IsActive = true }
            );

            modelBuilder.Entity<EmployeeProfile>()
                .HasKey(e => e.EmployeeProfileID);
            modelBuilder.Entity<EmployeeProfile>()
                .Property(e => e.EmployeeProfileID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeProfile>()
                .Property(e => e.EmploymentStatus)
                .HasConversion<int>();
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => new { e.SchoolID, e.EmployeeCode })
                .IsUnique();
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.SchoolID);
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.CurrentAcademicYearID);
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.EmployeeJobTypeID);
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.EmploymentStatus);
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.IsActive);
            modelBuilder.Entity<EmployeeProfile>()
                .HasIndex(e => e.UserId);
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.School)
                .WithMany()
                .HasForeignKey(e => e.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.CurrentAcademicYear)
                .WithMany()
                .HasForeignKey(e => e.CurrentAcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.JobType)
                .WithMany(j => j.EmployeeProfiles)
                .HasForeignKey(e => e.EmployeeJobTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            // SQL Server: Teacher→Manager is CASCADE; SET NULL on both Teacher and Manager from EmployeeProfiles
            // creates multiple cascade paths (error 1785). Use NO ACTION — clear legacy FKs in app code before deletes.
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeProfile>()
                .HasOne(e => e.SchoolStaff)
                .WithMany()
                .HasForeignKey(e => e.SchoolStaffID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeProfile>()
                .OwnsOne(e => e.FullName);
            modelBuilder.Entity<EmployeeProfile>()
                .OwnsOne(e => e.FullNameAlis);
            modelBuilder.Entity<EmployeeProfile>()
                .Navigation(e => e.FullNameAlis)
                .IsRequired(false);

            modelBuilder.Entity<EmployeeQualification>()
                .HasKey(q => q.EmployeeQualificationID);
            modelBuilder.Entity<EmployeeQualification>()
                .Property(q => q.EmployeeQualificationID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeQualification>()
                .HasOne(q => q.EmployeeProfile)
                .WithMany(p => p.Qualifications)
                .HasForeignKey(q => q.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeSpecialization>()
                .HasKey(s => s.EmployeeSpecializationID);
            modelBuilder.Entity<EmployeeSpecialization>()
                .Property(s => s.EmployeeSpecializationID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeSpecialization>()
                .HasOne(s => s.EmployeeProfile)
                .WithMany(p => p.Specializations)
                .HasForeignKey(s => s.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeHistory>()
                .HasKey(h => h.EmployeeHistoryID);
            modelBuilder.Entity<EmployeeHistory>()
                .Property(h => h.EmployeeHistoryID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeHistory>()
                .HasOne(h => h.EmployeeProfile)
                .WithMany(p => p.HistoryRecords)
                .HasForeignKey(h => h.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EmployeeHistory>()
                .HasOne(h => h.AcademicYear)
                .WithMany()
                .HasForeignKey(h => h.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeHistory>()
                .HasOne(h => h.School)
                .WithMany()
                .HasForeignKey(h => h.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeHistory>()
                .HasOne(h => h.JobType)
                .WithMany(j => j.EmployeeHistories)
                .HasForeignKey(h => h.EmployeeJobTypeID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<EmployeeHistory>()
                .HasIndex(h => new { h.EmployeeProfileID, h.AcademicYearID });

            modelBuilder.Entity<EmployeeDocument>()
                .HasKey(d => d.EmployeeDocumentID);
            modelBuilder.Entity<EmployeeDocument>()
                .Property(d => d.EmployeeDocumentID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeDocument>()
                .HasOne(d => d.EmployeeProfile)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeLeave>()
                .HasKey(l => l.EmployeeLeaveID);
            modelBuilder.Entity<EmployeeLeave>()
                .Property(l => l.EmployeeLeaveID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeLeave>()
                .Property(l => l.LeaveType)
                .HasConversion<int>();
            modelBuilder.Entity<EmployeeLeave>()
                .Property(l => l.ApprovalStatus)
                .HasConversion<int>();
            modelBuilder.Entity<EmployeeLeave>()
                .Property(l => l.TotalDays)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<EmployeeLeave>()
                .HasOne(l => l.EmployeeProfile)
                .WithMany(p => p.Leaves)
                .HasForeignKey(l => l.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EmployeeLeave>()
                .HasOne(l => l.AcademicYear)
                .WithMany()
                .HasForeignKey(l => l.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeLeave>()
                .HasIndex(l => new { l.EmployeeProfileID, l.AcademicYearID });

            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasKey(s => s.EmployeePerformanceSummaryID);
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .Property(s => s.EmployeePerformanceSummaryID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .Property(s => s.EvaluationScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasOne(s => s.EmployeeProfile)
                .WithMany(p => p.PerformanceSummaries)
                .HasForeignKey(s => s.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasOne(s => s.AcademicYear)
                .WithMany()
                .HasForeignKey(s => s.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasOne(s => s.School)
                .WithMany()
                .HasForeignKey(s => s.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasOne(s => s.JobType)
                .WithMany(j => j.PerformanceSummaries)
                .HasForeignKey(s => s.EmployeeJobTypeID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<EmployeePerformanceSummary>()
                .HasIndex(s => new { s.EmployeeProfileID, s.AcademicYearID, s.GeneratedAtUtc });

            // --- Recruitment / hiring ---
            modelBuilder.Entity<JobPosting>()
                .HasKey(j => j.JobPostingID);
            modelBuilder.Entity<JobPosting>()
                .Property(j => j.JobPostingID)
                .UseIdentityColumn();
            modelBuilder.Entity<JobPosting>()
                .Property(j => j.Status)
                .HasConversion<int>();
            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.SchoolID);
            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.AcademicYearID);
            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.EmployeeJobTypeID);
            modelBuilder.Entity<JobPosting>()
                .HasIndex(j => j.Status);
            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.School)
                .WithMany()
                .HasForeignKey(j => j.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.AcademicYear)
                .WithMany()
                .HasForeignKey(j => j.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.JobType)
                .WithMany()
                .HasForeignKey(j => j.EmployeeJobTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasKey(a => a.JobApplicationID);
            modelBuilder.Entity<JobApplication>()
                .Property(a => a.JobApplicationID)
                .UseIdentityColumn();
            modelBuilder.Entity<JobApplication>()
                .Property(a => a.Status)
                .HasConversion<int>();
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.JobPostingID);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.SchoolID);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.AcademicYearID);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.Status);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.Email);
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => a.NationalID);
            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.JobPosting)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.JobPostingID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.School)
                .WithMany()
                .HasForeignKey(a => a.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.AcademicYear)
                .WithMany()
                .HasForeignKey(a => a.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.ConvertedEmployeeProfile)
                .WithMany()
                .HasForeignKey(a => a.ConvertedEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Interview>()
                .ToTable("RecruitmentInterviews");
            modelBuilder.Entity<Interview>()
                .HasKey(i => i.InterviewID);
            modelBuilder.Entity<Interview>()
                .Property(i => i.InterviewID)
                .UseIdentityColumn();
            modelBuilder.Entity<Interview>()
                .Property(i => i.Status)
                .HasConversion<int>();
            modelBuilder.Entity<Interview>()
                .Property(i => i.Score)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Interview>()
                .HasIndex(i => i.JobApplicationID);
            modelBuilder.Entity<Interview>()
                .HasIndex(i => i.InterviewDate);
            modelBuilder.Entity<Interview>()
                .HasIndex(i => i.Status);
            modelBuilder.Entity<Interview>()
                .HasOne(i => i.JobApplication)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.JobApplicationID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Interview>()
                .HasOne(i => i.School)
                .WithMany()
                .HasForeignKey(i => i.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Interview>()
                .HasOne(i => i.AcademicYear)
                .WithMany()
                .HasForeignKey(i => i.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Interview>()
                .HasOne(i => i.InterviewerEmployeeProfile)
                .WithMany()
                .HasForeignKey(i => i.InterviewerEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CandidateEvaluation>()
                .HasKey(e => e.CandidateEvaluationID);
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.CandidateEvaluationID)
                .UseIdentityColumn();
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.Recommendation)
                .HasConversion<int>();
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.TechnicalScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.CommunicationScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.ClassManagementScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.CultureFitScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CandidateEvaluation>()
                .Property(e => e.OverallScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CandidateEvaluation>()
                .HasIndex(e => e.JobApplicationID);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasIndex(e => e.InterviewID);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasOne(e => e.JobApplication)
                .WithMany(a => a.Evaluations)
                .HasForeignKey(e => e.JobApplicationID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasOne(e => e.Interview)
                .WithMany(i => i.Evaluations)
                .HasForeignKey(e => e.InterviewID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasOne(e => e.School)
                .WithMany()
                .HasForeignKey(e => e.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasOne(e => e.AcademicYear)
                .WithMany()
                .HasForeignKey(e => e.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CandidateEvaluation>()
                .HasOne(e => e.EvaluatorEmployeeProfile)
                .WithMany()
                .HasForeignKey(e => e.EvaluatorEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HiringDecision>()
                .HasKey(d => d.HiringDecisionID);
            modelBuilder.Entity<HiringDecision>()
                .Property(d => d.HiringDecisionID)
                .UseIdentityColumn();
            modelBuilder.Entity<HiringDecision>()
                .Property(d => d.DecisionStatus)
                .HasConversion<int>();
            modelBuilder.Entity<HiringDecision>()
                .HasIndex(d => d.JobApplicationID)
                .IsUnique();
            modelBuilder.Entity<HiringDecision>()
                .HasIndex(d => d.DecisionStatus);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.JobApplication)
                .WithOne(a => a.HiringDecision)
                .HasForeignKey<HiringDecision>(d => d.JobApplicationID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.School)
                .WithMany()
                .HasForeignKey(d => d.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.AcademicYear)
                .WithMany()
                .HasForeignKey(d => d.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.OfferJobType)
                .WithMany()
                .HasForeignKey(d => d.OfferJobTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.DecidedByEmployeeProfile)
                .WithMany()
                .HasForeignKey(d => d.DecidedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HiringDecision>()
                .HasOne(d => d.ConvertedEmployeeProfile)
                .WithMany()
                .HasForeignKey(d => d.ConvertedEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Daily evaluation (employee performance) ---
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .HasKey(t => t.DailyEvaluationTemplateID);
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .Property(t => t.DailyEvaluationTemplateID)
                .UseIdentityColumn();
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .Property(t => t.Status)
                .HasConversion<int>();
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .HasIndex(t => new { t.SchoolID, t.AcademicYearID, t.EmployeeJobTypeID, t.Status });
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .HasOne(t => t.School)
                .WithMany()
                .HasForeignKey(t => t.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .HasOne(t => t.AcademicYear)
                .WithMany()
                .HasForeignKey(t => t.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluationTemplate>()
                .HasOne(t => t.JobType)
                .WithMany()
                .HasForeignKey(t => t.EmployeeJobTypeID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DailyEvaluationCriteria>()
                .HasKey(c => c.DailyEvaluationCriteriaID);
            modelBuilder.Entity<DailyEvaluationCriteria>()
                .Property(c => c.DailyEvaluationCriteriaID)
                .UseIdentityColumn();
            modelBuilder.Entity<DailyEvaluationCriteria>()
                .HasIndex(c => new { c.DailyEvaluationTemplateID, c.SortOrder, c.IsActive });
            modelBuilder.Entity<DailyEvaluationCriteria>()
                .HasOne(c => c.Template)
                .WithMany(t => t.Criteria)
                .HasForeignKey(c => c.DailyEvaluationTemplateID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyEvaluation>()
                .HasKey(e => e.DailyEvaluationID);
            modelBuilder.Entity<DailyEvaluation>()
                .Property(e => e.DailyEvaluationID)
                .UseIdentityColumn();
            modelBuilder.Entity<DailyEvaluation>()
                .Property(e => e.Status)
                .HasConversion<int>();
            modelBuilder.Entity<DailyEvaluation>()
                .Property(e => e.TotalScore)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DailyEvaluation>()
                .HasIndex(e => new { e.SchoolID, e.AcademicYearID, e.EvaluatedEmployeeProfileID, e.EvaluationDate, e.DailyEvaluationTemplateID, e.IsLocked });
            modelBuilder.Entity<DailyEvaluation>()
                .HasIndex(e => new { e.EvaluatedEmployeeProfileID, e.EvaluationDate, e.DailyEvaluationTemplateID })
                .IsUnique();
            modelBuilder.Entity<DailyEvaluation>()
                .HasOne(e => e.School)
                .WithMany()
                .HasForeignKey(e => e.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluation>()
                .HasOne(e => e.AcademicYear)
                .WithMany()
                .HasForeignKey(e => e.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluation>()
                .HasOne(e => e.EvaluatedEmployeeProfile)
                .WithMany(p => p.DailyEvaluationsAsEvaluated)
                .HasForeignKey(e => e.EvaluatedEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluation>()
                .HasOne(e => e.EvaluatorEmployeeProfile)
                .WithMany(p => p.DailyEvaluationsAsEvaluator)
                .HasForeignKey(e => e.EvaluatorEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyEvaluation>()
                .HasOne(e => e.Template)
                .WithMany(t => t.DailyEvaluations)
                .HasForeignKey(e => e.DailyEvaluationTemplateID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyEvaluationItem>()
                .HasKey(i => i.DailyEvaluationItemID);
            modelBuilder.Entity<DailyEvaluationItem>()
                .Property(i => i.DailyEvaluationItemID)
                .UseIdentityColumn();
            modelBuilder.Entity<DailyEvaluationItem>()
                .Property(i => i.Score)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DailyEvaluationItem>()
                .HasIndex(i => new { i.DailyEvaluationID, i.DailyEvaluationCriteriaID })
                .IsUnique();
            modelBuilder.Entity<DailyEvaluationItem>()
                .HasOne(i => i.DailyEvaluation)
                .WithMany(e => e.Items)
                .HasForeignKey(i => i.DailyEvaluationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DailyEvaluationItem>()
                .HasOne(i => i.Criteria)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.DailyEvaluationCriteriaID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EvaluationLock>()
                .HasKey(l => l.EvaluationLockID);
            modelBuilder.Entity<EvaluationLock>()
                .Property(l => l.EvaluationLockID)
                .UseIdentityColumn();
            modelBuilder.Entity<EvaluationLock>()
                .Property(l => l.Status)
                .HasConversion<int>();
            modelBuilder.Entity<EvaluationLock>()
                .HasIndex(l => new { l.SchoolID, l.AcademicYearID, l.LockDate, l.Status });
            modelBuilder.Entity<EvaluationLock>()
                .HasIndex(l => new { l.SchoolID, l.AcademicYearID, l.LockDate, l.DailyEvaluationTemplateID })
                .IsUnique();
            modelBuilder.Entity<EvaluationLock>()
                .HasOne(l => l.School)
                .WithMany()
                .HasForeignKey(l => l.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EvaluationLock>()
                .HasOne(l => l.AcademicYear)
                .WithMany()
                .HasForeignKey(l => l.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EvaluationLock>()
                .HasOne(l => l.Template)
                .WithMany()
                .HasForeignKey(l => l.DailyEvaluationTemplateID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasKey(x => x.EvaluationOverrideLogID);
            modelBuilder.Entity<EvaluationOverrideLog>()
                .Property(x => x.EvaluationOverrideLogID)
                .UseIdentityColumn();
            modelBuilder.Entity<EvaluationOverrideLog>()
                .Property(x => x.OverrideActionType)
                .HasConversion<int>();
            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasIndex(x => new { x.DailyEvaluationID, x.EvaluationLockID, x.PerformedAtUtc });
            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasOne(x => x.DailyEvaluation)
                .WithMany(e => e.OverrideLogs)
                .HasForeignKey(x => x.DailyEvaluationID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasOne(x => x.EvaluationLock)
                .WithMany(l => l.OverrideLogs)
                .HasForeignKey(x => x.EvaluationLockID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EvaluationOverrideLog>()
                .HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Supervisor visits (زيارات المشرف) ---
            modelBuilder.Entity<SupervisorVisit>()
                .HasKey(v => v.SupervisorVisitID);
            modelBuilder.Entity<SupervisorVisit>()
                .Property(v => v.SupervisorVisitID)
                .UseIdentityColumn();
            modelBuilder.Entity<SupervisorVisit>()
                .Property(v => v.Status)
                .HasConversion<int>();
            modelBuilder.Entity<SupervisorVisit>()
                .Property(v => v.OverallScoreOutOf100)
                .HasColumnType("decimal(5,2)");
            modelBuilder.Entity<SupervisorVisit>()
                .HasIndex(v => new { v.SchoolID, v.AcademicYearID, v.VisitedTeacherID, v.VisitDate });
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.School)
                .WithMany()
                .HasForeignKey(v => v.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.AcademicYear)
                .WithMany()
                .HasForeignKey(v => v.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.VisitedTeacher)
                .WithMany(t => t.SupervisorVisitsReceived)
                .HasForeignKey(v => v.VisitedTeacherID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.Class)
                .WithMany()
                .HasForeignKey(v => v.ClassID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.Subject)
                .WithMany()
                .HasForeignKey(v => v.SubjectID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupervisorVisit>()
                .HasOne(v => v.SupervisorEmployeeProfile)
                .WithMany(p => p.SupervisorVisitsConducted)
                .HasForeignKey(v => v.SupervisorEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitObservation>()
                .HasKey(o => o.VisitObservationID);
            modelBuilder.Entity<VisitObservation>()
                .Property(o => o.VisitObservationID)
                .UseIdentityColumn();
            modelBuilder.Entity<VisitObservation>()
                .HasIndex(o => new { o.SupervisorVisitID, o.SortOrder });
            modelBuilder.Entity<VisitObservation>()
                .HasOne(o => o.SupervisorVisit)
                .WithMany(v => v.Observations)
                .HasForeignKey(o => o.SupervisorVisitID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitRecommendation>()
                .HasKey(r => r.VisitRecommendationID);
            modelBuilder.Entity<VisitRecommendation>()
                .Property(r => r.VisitRecommendationID)
                .UseIdentityColumn();
            modelBuilder.Entity<VisitRecommendation>()
                .Property(r => r.ImplementationStatus)
                .HasConversion<int>();
            modelBuilder.Entity<VisitRecommendation>()
                .HasIndex(r => new { r.SupervisorVisitID, r.SortOrder });
            modelBuilder.Entity<VisitRecommendation>()
                .HasOne(r => r.SupervisorVisit)
                .WithMany(v => v.Recommendations)
                .HasForeignKey(r => r.SupervisorVisitID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecommendationFollowUp>()
                .HasKey(f => f.RecommendationFollowUpID);
            modelBuilder.Entity<RecommendationFollowUp>()
                .Property(f => f.RecommendationFollowUpID)
                .UseIdentityColumn();
            modelBuilder.Entity<RecommendationFollowUp>()
                .HasIndex(f => new { f.VisitRecommendationID, f.FollowUpDate, f.CreatedAtUtc });
            modelBuilder.Entity<RecommendationFollowUp>()
                .HasOne(f => f.VisitRecommendation)
                .WithMany(r => r.FollowUps)
                .HasForeignKey(f => f.VisitRecommendationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RecommendationFollowUp>()
                .HasOne(f => f.FollowUpByEmployeeProfile)
                .WithMany(p => p.RecommendationFollowUpsAuthored)
                .HasForeignKey(f => f.FollowUpByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Teacher performance feedback (students & parents) ---
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .HasKey(x => x.TeacherFeedbackCycleID);
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .Property(x => x.TeacherFeedbackCycleID)
                .UseIdentityColumn();
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .Property(x => x.Status)
                .HasConversion<int>();
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .HasIndex(x => new { x.SchoolID, x.AcademicYearID, x.TeacherID, x.Status });
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TeacherFeedbackCycle>()
                .HasOne(x => x.Teacher)
                .WithMany(t => t.TeacherFeedbackCycles)
                .HasForeignKey(x => x.TeacherID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeedbackQuestion>()
                .HasKey(q => q.FeedbackQuestionID);
            modelBuilder.Entity<FeedbackQuestion>()
                .Property(q => q.FeedbackQuestionID)
                .UseIdentityColumn();
            modelBuilder.Entity<FeedbackQuestion>()
                .Property(q => q.QuestionType)
                .HasConversion<int>();
            modelBuilder.Entity<FeedbackQuestion>()
                .Property(q => q.Audience)
                .HasConversion<int>();
            modelBuilder.Entity<FeedbackQuestion>()
                .HasIndex(q => new { q.TeacherFeedbackCycleID, q.SortOrder });
            modelBuilder.Entity<FeedbackQuestion>()
                .HasOne(q => q.TeacherFeedbackCycle)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.TeacherFeedbackCycleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentFeedback>()
                .HasKey(s => s.StudentFeedbackID);
            modelBuilder.Entity<StudentFeedback>()
                .Property(s => s.StudentFeedbackID)
                .UseIdentityColumn();
            modelBuilder.Entity<StudentFeedback>()
                .Property(s => s.Status)
                .HasConversion<int>();
            modelBuilder.Entity<StudentFeedback>()
                .HasIndex(s => new { s.TeacherFeedbackCycleID, s.StudentID })
                .IsUnique();
            modelBuilder.Entity<StudentFeedback>()
                .HasOne(s => s.TeacherFeedbackCycle)
                .WithMany(c => c.StudentFeedbacks)
                .HasForeignKey(s => s.TeacherFeedbackCycleID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StudentFeedback>()
                .HasOne(s => s.Student)
                .WithMany(st => st.StudentFeedbacks)
                .HasForeignKey(s => s.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParentFeedback>()
                .HasKey(p => p.ParentFeedbackID);
            modelBuilder.Entity<ParentFeedback>()
                .Property(p => p.ParentFeedbackID)
                .UseIdentityColumn();
            modelBuilder.Entity<ParentFeedback>()
                .Property(p => p.Status)
                .HasConversion<int>();
            modelBuilder.Entity<ParentFeedback>()
                .HasIndex(p => new { p.TeacherFeedbackCycleID, p.GuardianID, p.StudentID })
                .IsUnique();
            modelBuilder.Entity<ParentFeedback>()
                .HasOne(p => p.TeacherFeedbackCycle)
                .WithMany(c => c.ParentFeedbacks)
                .HasForeignKey(p => p.TeacherFeedbackCycleID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ParentFeedback>()
                .HasOne(p => p.Guardian)
                .WithMany(g => g.ParentFeedbacks)
                .HasForeignKey(p => p.GuardianID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ParentFeedback>()
                .HasOne(p => p.Student)
                .WithMany(st => st.ParentFeedbacksAsSubject)
                .HasForeignKey(p => p.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeedbackSummary>()
                .HasKey(s => s.FeedbackSummaryID);
            modelBuilder.Entity<FeedbackSummary>()
                .Property(s => s.FeedbackSummaryID)
                .UseIdentityColumn();
            modelBuilder.Entity<FeedbackSummary>()
                .Property(s => s.Audience)
                .HasConversion<int>();
            modelBuilder.Entity<FeedbackSummary>()
                .Property(s => s.AverageNumericScore)
                .HasColumnType("decimal(6,3)");
            modelBuilder.Entity<FeedbackSummary>()
                .HasIndex(s => new { s.TeacherFeedbackCycleID, s.Audience })
                .IsUnique();
            modelBuilder.Entity<FeedbackSummary>()
                .HasOne(s => s.TeacherFeedbackCycle)
                .WithMany(c => c.Summaries)
                .HasForeignKey(s => s.TeacherFeedbackCycleID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Employee requests: type, request, approvals, execution updates, daily summaries ---
            modelBuilder.Entity<RequestType>()
                .ToTable("EmployeeRequestTypes");
            modelBuilder.Entity<RequestType>()
                .HasKey(x => x.RequestTypeID);
            modelBuilder.Entity<RequestType>()
                .Property(x => x.RequestTypeID)
                .UseIdentityColumn();
            modelBuilder.Entity<RequestType>()
                .Property(x => x.Category)
                .HasConversion<int>();
            modelBuilder.Entity<RequestType>()
                .HasIndex(x => new { x.SchoolID, x.Code })
                .IsUnique();
            modelBuilder.Entity<RequestType>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeRequest>()
                .ToTable("StaffEmployeeRequests");
            modelBuilder.Entity<EmployeeRequest>()
                .HasKey(x => x.EmployeeRequestID);
            modelBuilder.Entity<EmployeeRequest>()
                .Property(x => x.EmployeeRequestID)
                .UseIdentityColumn();
            modelBuilder.Entity<EmployeeRequest>()
                .Property(x => x.Status)
                .HasConversion<int>();
            modelBuilder.Entity<EmployeeRequest>()
                .Property(x => x.RequestedAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<EmployeeRequest>()
                .HasIndex(x => new { x.SchoolID, x.AcademicYearID, x.EmployeeProfileID, x.Status });
            modelBuilder.Entity<EmployeeRequest>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeRequest>()
                .HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeRequest>()
                .HasOne(x => x.EmployeeProfile)
                .WithMany(p => p.EmployeeRequests)
                .HasForeignKey(x => x.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeRequest>()
                .HasOne(x => x.RequestType)
                .WithMany(t => t.EmployeeRequests)
                .HasForeignKey(x => x.RequestTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestApprovalStep>()
                .ToTable("StaffRequestApprovalSteps");
            modelBuilder.Entity<RequestApprovalStep>()
                .HasKey(x => x.RequestApprovalStepID);
            modelBuilder.Entity<RequestApprovalStep>()
                .Property(x => x.RequestApprovalStepID)
                .UseIdentityColumn();
            modelBuilder.Entity<RequestApprovalStep>()
                .Property(x => x.Decision)
                .HasConversion<int>();
            modelBuilder.Entity<RequestApprovalStep>()
                .HasIndex(x => new { x.EmployeeRequestID, x.StepOrder });
            modelBuilder.Entity<RequestApprovalStep>()
                .HasOne(x => x.EmployeeRequest)
                .WithMany(r => r.ApprovalSteps)
                .HasForeignKey(x => x.EmployeeRequestID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RequestApprovalStep>()
                .HasOne(x => x.ApproverEmployeeProfile)
                .WithMany(p => p.RequestApprovalStepsAsApprover)
                .HasForeignKey(x => x.ApproverEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestExecution>()
                .ToTable("StaffRequestExecutions");
            modelBuilder.Entity<RequestExecution>()
                .HasKey(x => x.RequestExecutionID);
            modelBuilder.Entity<RequestExecution>()
                .Property(x => x.RequestExecutionID)
                .UseIdentityColumn();
            modelBuilder.Entity<RequestExecution>()
                .Property(x => x.Status)
                .HasConversion<int>();
            modelBuilder.Entity<RequestExecution>()
                .HasIndex(x => new { x.EmployeeRequestID, x.UpdatedAtUtc });
            modelBuilder.Entity<RequestExecution>()
                .HasOne(x => x.EmployeeRequest)
                .WithMany(r => r.Executions)
                .HasForeignKey(x => x.EmployeeRequestID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RequestExecution>()
                .HasOne(x => x.ResponsibleEmployeeProfile)
                .WithMany(p => p.RequestExecutionsAsResponsible)
                .HasForeignKey(x => x.ResponsibleEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequestDailySummary>()
                .ToTable("StaffRequestDailySummaries");
            modelBuilder.Entity<RequestDailySummary>()
                .HasKey(x => x.RequestDailySummaryID);
            modelBuilder.Entity<RequestDailySummary>()
                .Property(x => x.RequestDailySummaryID)
                .UseIdentityColumn();
            modelBuilder.Entity<RequestDailySummary>()
                .HasIndex(x => new { x.EmployeeRequestID, x.SummaryDate, x.CreatedAtUtc });
            modelBuilder.Entity<RequestDailySummary>()
                .HasOne(x => x.EmployeeRequest)
                .WithMany(r => r.DailySummaries)
                .HasForeignKey(x => x.EmployeeRequestID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RequestDailySummary>()
                .HasOne(x => x.CreatedByEmployeeProfile)
                .WithMany(p => p.RequestDailySummariesAuthored)
                .HasForeignKey(x => x.CreatedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Achievements: catalog, requests, approvals, attachments, points ledger ---
            modelBuilder.Entity<Achievement>()
                .HasKey(a => a.AchievementID);
            modelBuilder.Entity<Achievement>()
                .Property(a => a.AchievementID)
                .UseIdentityColumn();
            modelBuilder.Entity<Achievement>()
                .HasIndex(a => new { a.SchoolID, a.Code })
                .IsUnique();
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.School)
                .WithMany()
                .HasForeignKey(a => a.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.AcademicYear)
                .WithMany()
                .HasForeignKey(a => a.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AchievementRequest>()
                .HasKey(r => r.AchievementRequestID);
            modelBuilder.Entity<AchievementRequest>()
                .Property(r => r.AchievementRequestID)
                .UseIdentityColumn();
            modelBuilder.Entity<AchievementRequest>()
                .Property(r => r.Status)
                .HasConversion<int>();
            modelBuilder.Entity<AchievementRequest>()
                .HasIndex(r => new { r.SchoolID, r.AcademicYearID, r.EmployeeProfileID, r.Status });
            modelBuilder.Entity<AchievementRequest>()
                .HasOne(r => r.School)
                .WithMany()
                .HasForeignKey(r => r.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementRequest>()
                .HasOne(r => r.AcademicYear)
                .WithMany()
                .HasForeignKey(r => r.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementRequest>()
                .HasOne(r => r.EmployeeProfile)
                .WithMany(p => p.AchievementRequests)
                .HasForeignKey(r => r.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementRequest>()
                .HasOne(r => r.Achievement)
                .WithMany(a => a.Requests)
                .HasForeignKey(r => r.AchievementID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AchievementApproval>()
                .HasKey(x => x.AchievementApprovalID);
            modelBuilder.Entity<AchievementApproval>()
                .Property(x => x.AchievementApprovalID)
                .UseIdentityColumn();
            modelBuilder.Entity<AchievementApproval>()
                .Property(x => x.Decision)
                .HasConversion<int>();
            modelBuilder.Entity<AchievementApproval>()
                .HasIndex(x => new { x.AchievementRequestID, x.SortOrder });
            modelBuilder.Entity<AchievementApproval>()
                .HasOne(x => x.AchievementRequest)
                .WithMany(r => r.Approvals)
                .HasForeignKey(x => x.AchievementRequestID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AchievementApproval>()
                .HasOne(x => x.ApproverEmployeeProfile)
                .WithMany(p => p.AchievementApprovalsAsApprover)
                .HasForeignKey(x => x.ApproverEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AchievementAttachment>()
                .HasKey(x => x.AchievementAttachmentID);
            modelBuilder.Entity<AchievementAttachment>()
                .Property(x => x.AchievementAttachmentID)
                .UseIdentityColumn();
            modelBuilder.Entity<AchievementAttachment>()
                .HasOne(x => x.AchievementRequest)
                .WithMany(r => r.Attachments)
                .HasForeignKey(x => x.AchievementRequestID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AchievementAttachment>()
                .HasOne(x => x.UploadedByEmployeeProfile)
                .WithMany(p => p.AchievementAttachmentsUploaded)
                .HasForeignKey(x => x.UploadedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AchievementPointsLedger>()
                .HasKey(x => x.AchievementPointsLedgerID);
            modelBuilder.Entity<AchievementPointsLedger>()
                .Property(x => x.AchievementPointsLedgerID)
                .UseIdentityColumn();
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasIndex(x => new { x.EmployeeProfileID, x.AcademicYearID, x.CreatedAtUtc });
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasOne(x => x.EmployeeProfile)
                .WithMany(p => p.AchievementPointsLedgerEntries)
                .HasForeignKey(x => x.EmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasOne(x => x.AchievementRequest)
                .WithMany()
                .HasForeignKey(x => x.AchievementRequestID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<AchievementPointsLedger>()
                .HasOne(x => x.CreatedByEmployeeProfile)
                .WithMany(p => p.AchievementPointsLedgerEntriesCreated)
                .HasForeignKey(x => x.CreatedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Violations (HR): types, case, responses, actions, escalation history ---
            modelBuilder.Entity<ViolationType>()
                .HasKey(x => x.ViolationTypeID);
            modelBuilder.Entity<ViolationType>()
                .Property(x => x.ViolationTypeID)
                .UseIdentityColumn();
            modelBuilder.Entity<ViolationType>()
                .Property(x => x.Kind)
                .HasConversion<int>();
            modelBuilder.Entity<ViolationType>()
                .HasIndex(x => new { x.SchoolID, x.Kind })
                .IsUnique();
            modelBuilder.Entity<ViolationType>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Violation>()
                .HasKey(x => x.ViolationID);
            modelBuilder.Entity<Violation>()
                .Property(x => x.ViolationID)
                .UseIdentityColumn();
            modelBuilder.Entity<Violation>()
                .Property(x => x.Status)
                .HasConversion<int>();
            modelBuilder.Entity<Violation>()
                .HasIndex(x => new { x.SchoolID, x.SubjectEmployeeProfileID, x.Status });
            modelBuilder.Entity<Violation>()
                .HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Violation>()
                .HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Violation>()
                .HasOne(x => x.ViolationType)
                .WithMany(t => t.Violations)
                .HasForeignKey(x => x.ViolationTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Violation>()
                .HasOne(x => x.SubjectEmployeeProfile)
                .WithMany(p => p.ViolationsAsSubject)
                .HasForeignKey(x => x.SubjectEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Violation>()
                .HasOne(x => x.OpenedByEmployeeProfile)
                .WithMany(p => p.ViolationsOpened)
                .HasForeignKey(x => x.OpenedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViolationResponse>()
                .HasKey(x => x.ViolationResponseID);
            modelBuilder.Entity<ViolationResponse>()
                .Property(x => x.ViolationResponseID)
                .UseIdentityColumn();
            modelBuilder.Entity<ViolationResponse>()
                .HasOne(x => x.Violation)
                .WithMany(v => v.Responses)
                .HasForeignKey(x => x.ViolationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ViolationResponse>()
                .HasOne(x => x.AuthorEmployeeProfile)
                .WithMany(p => p.ViolationResponses)
                .HasForeignKey(x => x.AuthorEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViolationAction>()
                .HasKey(x => x.ViolationActionID);
            modelBuilder.Entity<ViolationAction>()
                .Property(x => x.ViolationActionID)
                .UseIdentityColumn();
            modelBuilder.Entity<ViolationAction>()
                .Property(x => x.Category)
                .HasConversion<int>();
            modelBuilder.Entity<ViolationAction>()
                .HasOne(x => x.Violation)
                .WithMany(v => v.Actions)
                .HasForeignKey(x => x.ViolationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ViolationAction>()
                .HasOne(x => x.PerformedByEmployeeProfile)
                .WithMany(p => p.ViolationActionsPerformed)
                .HasForeignKey(x => x.PerformedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasKey(x => x.ViolationEscalationHistoryID);
            modelBuilder.Entity<ViolationEscalationHistory>()
                .Property(x => x.ViolationEscalationHistoryID)
                .UseIdentityColumn();
            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasIndex(x => new { x.ViolationID, x.ChangedAtUtc });
            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasOne(x => x.Violation)
                .WithMany(v => v.EscalationHistory)
                .HasForeignKey(x => x.ViolationID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasOne(x => x.PreviousViolationType)
                .WithMany(t => t.EscalationHistoriesFrom)
                .HasForeignKey(x => x.PreviousViolationTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasOne(x => x.NewViolationType)
                .WithMany(t => t.EscalationHistoriesTo)
                .HasForeignKey(x => x.NewViolationTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ViolationEscalationHistory>()
                .HasOne(x => x.ChangedByEmployeeProfile)
                .WithMany(p => p.ViolationEscalationsChanged)
                .HasForeignKey(x => x.ChangedByEmployeeProfileID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamType>().HasData(
                new ExamType { ExamTypeID = 1, Name = "Midterm", SortOrder = 1, IsActive = true },
                new ExamType { ExamTypeID = 2, Name = "Final", SortOrder = 2, IsActive = true },
                new ExamType { ExamTypeID = 3, Name = "Quiz", SortOrder = 3, IsActive = true },
                new ExamType { ExamTypeID = 4, Name = "Oral", SortOrder = 4, IsActive = true },
                new ExamType { ExamTypeID = 5, Name = "Practical", SortOrder = 5, IsActive = true },
                new ExamType { ExamTypeID = 6, Name = "Makeup", SortOrder = 6, IsActive = true }
            );

            // Configure unique index on Code + SchoolId (allows same code for different schools, but unique per school)
            modelBuilder.Entity<ReportTemplate>()
                .HasIndex(rt => new { rt.Code, rt.SchoolId })
                .IsUnique()
                .HasFilter("[SchoolId] IS NOT NULL");

            // Configure unique index on Code when SchoolId is null (global templates)
            modelBuilder.Entity<ReportTemplate>()
                .HasIndex(rt => rt.Code)
                .IsUnique()
                .HasFilter("[SchoolId] IS NULL");

            // Configure relationship with School (optional)
            modelBuilder.Entity<ReportTemplate>()
                .HasOne(rt => rt.School)
                .WithMany()
                .HasForeignKey(rt => rt.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure owned entities (composite types)
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.FullName);
            
            // Optional English names — API may omit FullNameAlis when no English name is provided.
            modelBuilder.Entity<Student>()
                .OwnsOne(s => s.FullNameAlis);
            modelBuilder.Entity<Student>()
                .Navigation(s => s.FullNameAlis)
                .IsRequired(false);
            
            modelBuilder.Entity<Teacher>()
                .OwnsOne(T => T.FullName);
            
            modelBuilder.Entity<Manager>()
                .OwnsOne(M => M.FullName);

            modelBuilder.Entity<SchoolStaff>()
                .OwnsOne(s => s.FullName);

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
