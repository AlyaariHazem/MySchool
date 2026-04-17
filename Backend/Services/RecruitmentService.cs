using Backend.Data;
using Backend.DTOS.School.Employees;
using Backend.DTOS.School.Recruitment;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly TenantDbContext _db;
    private readonly IEmployeeProfileService _employees;

    public RecruitmentService(TenantDbContext db, IEmployeeProfileService employees)
    {
        _db = db;
        _employees = employees;
    }

    private async Task EnsureYearBelongsToSchoolAsync(int schoolId, int yearId, CancellationToken cancellationToken)
    {
        var ok = await _db.Years.AsNoTracking()
            .AnyAsync(y => y.YearID == yearId && y.SchoolID == schoolId, cancellationToken);
        if (!ok)
            throw new ArgumentException($"Academic year {yearId} is not valid for school {schoolId}.");
    }

    private async Task<EmployeeJobType> GetJobTypeAsync(int id, CancellationToken cancellationToken) =>
        await _db.EmployeeJobTypes.AsNoTracking()
                   .FirstOrDefaultAsync(j => j.EmployeeJobTypeID == id, cancellationToken)
        ?? throw new KeyNotFoundException($"Employee job type {id} was not found.");

    private static void ValidatePostingDates(DateTime postingDate, DateTime? closingDate)
    {
        if (closingDate.HasValue && closingDate.Value.Date < postingDate.Date)
            throw new ArgumentException("Closing date cannot be before posting date.");
    }

    private static bool IsTerminalApplicationStatus(JobApplicationStatus s) =>
        s is JobApplicationStatus.Rejected or JobApplicationStatus.Withdrawn
            or JobApplicationStatus.ConvertedToEmployee;

    private static bool BlocksInterviews(JobApplicationStatus s) =>
        s is JobApplicationStatus.Rejected or JobApplicationStatus.Withdrawn;

    public async Task<JobPostingReadDto> CreateJobPostingAsync(JobPostingCreateDto dto, CancellationToken cancellationToken = default)
    {
        await GetJobTypeAsync(dto.EmployeeJobTypeID, cancellationToken);
        if (dto.AcademicYearID is int y && y > 0)
            await EnsureYearBelongsToSchoolAsync(dto.SchoolID, y, cancellationToken);
        ValidatePostingDates(dto.PostingDate, dto.ClosingDate);

        var entity = new JobPosting
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            EmployeeJobTypeID = dto.EmployeeJobTypeID,
            Title = dto.Title.Trim(),
            Department = dto.Department?.Trim(),
            Description = dto.Description,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            EmploymentType = dto.EmploymentType,
            NumberOfOpenings = dto.NumberOfOpenings,
            PostingDate = dto.PostingDate,
            ClosingDate = dto.ClosingDate,
            Status = dto.Status,
            Notes = dto.Notes,
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.JobPostings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobPostingByIdAsync(entity.JobPostingID, cancellationToken))!;
    }

    public async Task<JobPostingReadDto> UpdateJobPostingAsync(int id, JobPostingUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobPostings.FirstOrDefaultAsync(p => p.JobPostingID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job posting {id} was not found.");
        await GetJobTypeAsync(dto.EmployeeJobTypeID, cancellationToken);
        if (dto.AcademicYearID is int y2 && y2 > 0)
            await EnsureYearBelongsToSchoolAsync(dto.SchoolID, y2, cancellationToken);
        ValidatePostingDates(dto.PostingDate, dto.ClosingDate);

        entity.SchoolID = dto.SchoolID;
        entity.AcademicYearID = dto.AcademicYearID;
        entity.EmployeeJobTypeID = dto.EmployeeJobTypeID;
        entity.Title = dto.Title.Trim();
        entity.Department = dto.Department?.Trim();
        entity.Description = dto.Description;
        entity.Requirements = dto.Requirements;
        entity.Responsibilities = dto.Responsibilities;
        entity.EmploymentType = dto.EmploymentType;
        entity.NumberOfOpenings = dto.NumberOfOpenings;
        entity.PostingDate = dto.PostingDate;
        entity.ClosingDate = dto.ClosingDate;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobPostingByIdAsync(id, cancellationToken))!;
    }

    public async Task<IReadOnlyList<JobPostingListDto>> GetJobPostingsAsync(JobPostingFilterDto? filter, CancellationToken cancellationToken = default)
    {
        var q = _db.JobPostings.AsNoTracking().Include(p => p.JobType).AsQueryable();
        if (filter?.SchoolID is > 0)
            q = q.Where(p => p.SchoolID == filter.SchoolID);
        if (filter?.AcademicYearID is > 0)
            q = q.Where(p => p.AcademicYearID == filter.AcademicYearID);
        if (filter?.EmployeeJobTypeID is > 0)
            q = q.Where(p => p.EmployeeJobTypeID == filter.EmployeeJobTypeID);
        if (filter?.Status is { } st)
            q = q.Where(p => p.Status == st);
        if (filter?.IsActive is bool ia)
            q = q.Where(p => p.IsActive == ia);

        return await q.OrderByDescending(p => p.PostingDate).ThenBy(p => p.Title)
            .Select(p => new JobPostingListDto
            {
                JobPostingID = p.JobPostingID,
                SchoolID = p.SchoolID,
                AcademicYearID = p.AcademicYearID,
                Title = p.Title,
                Department = p.Department,
                Status = p.Status,
                PostingDate = p.PostingDate,
                ClosingDate = p.ClosingDate,
                NumberOfOpenings = p.NumberOfOpenings,
                JobTypeName = p.JobType.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<JobPostingReadDto?> GetJobPostingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _db.JobPostings.AsNoTracking()
            .Include(x => x.JobType)
            .FirstOrDefaultAsync(x => x.JobPostingID == id, cancellationToken);
        if (p == null) return null;
        return new JobPostingReadDto
        {
            JobPostingID = p.JobPostingID,
            SchoolID = p.SchoolID,
            AcademicYearID = p.AcademicYearID,
            EmployeeJobTypeID = p.EmployeeJobTypeID,
            JobTypeCode = p.JobType.Code,
            JobTypeName = p.JobType.Name,
            Title = p.Title,
            Department = p.Department,
            Description = p.Description,
            Requirements = p.Requirements,
            Responsibilities = p.Responsibilities,
            EmploymentType = p.EmploymentType,
            NumberOfOpenings = p.NumberOfOpenings,
            PostingDate = p.PostingDate,
            ClosingDate = p.ClosingDate,
            Status = p.Status,
            Notes = p.Notes,
            IsActive = p.IsActive,
            CreatedAtUtc = p.CreatedAtUtc,
            UpdatedAtUtc = p.UpdatedAtUtc
        };
    }

    public async Task<JobPostingReadDto> OpenJobPostingAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobPostings.FirstOrDefaultAsync(p => p.JobPostingID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job posting {id} was not found.");
        if (entity.Status is not (JobPostingStatus.Draft or JobPostingStatus.Closed))
            throw new InvalidOperationException("Only draft or closed postings can be opened.");
        entity.Status = JobPostingStatus.Open;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobPostingByIdAsync(id, cancellationToken))!;
    }

    public async Task<JobPostingReadDto> CloseJobPostingAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobPostings.FirstOrDefaultAsync(p => p.JobPostingID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job posting {id} was not found.");
        if (entity.Status != JobPostingStatus.Open)
            throw new InvalidOperationException("Only open postings can be closed.");
        entity.Status = JobPostingStatus.Closed;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobPostingByIdAsync(id, cancellationToken))!;
    }

    public async Task<JobPostingReadDto> ArchiveJobPostingAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobPostings.FirstOrDefaultAsync(p => p.JobPostingID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job posting {id} was not found.");
        entity.Status = JobPostingStatus.Archived;
        entity.IsActive = false;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobPostingByIdAsync(id, cancellationToken))!;
    }

    public async Task<JobApplicationReadDto> CreateJobApplicationAsync(JobApplicationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var posting = await _db.JobPostings.FirstOrDefaultAsync(p => p.JobPostingID == dto.JobPostingID, cancellationToken)
                      ?? throw new KeyNotFoundException($"Job posting {dto.JobPostingID} was not found.");
        if (posting.Status != JobPostingStatus.Open)
            throw new InvalidOperationException("Applications can only be submitted to open postings.");
        if (posting.ClosingDate.HasValue && posting.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("This posting is past its closing date.");

        int academicYearId;
        if (posting.AcademicYearID is int py && py > 0)
        {
            academicYearId = py;
            if (dto.AcademicYearID is int dtoY && dtoY > 0 && dtoY != py)
                throw new ArgumentException("Academic year must match the job posting's academic year.");
        }
        else
        {
            if (dto.AcademicYearID is not int ay || ay <= 0)
                throw new ArgumentException("AcademicYearID is required when the posting does not specify a year.");
            academicYearId = ay;
        }

        await EnsureYearBelongsToSchoolAsync(posting.SchoolID, academicYearId, cancellationToken);

        var entity = new JobApplication
        {
            JobPostingID = posting.JobPostingID,
            SchoolID = posting.SchoolID,
            AcademicYearID = academicYearId,
            ApplicantFirstName = dto.ApplicantFirstName.Trim(),
            ApplicantLastName = dto.ApplicantLastName.Trim(),
            ApplicantArabicName = dto.ApplicantArabicName?.Trim(),
            ApplicantEnglishName = dto.ApplicantEnglishName?.Trim(),
            NationalID = dto.NationalID?.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Phone = dto.Phone,
            Email = dto.Email?.Trim(),
            Address = dto.Address,
            HighestQualification = dto.HighestQualification,
            Specialization = dto.Specialization,
            YearsOfExperience = dto.YearsOfExperience,
            CurrentEmployer = dto.CurrentEmployer,
            ResumeFileUrl = dto.ResumeFileUrl,
            CoverLetter = dto.CoverLetter,
            Source = dto.Source,
            Status = JobApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.JobApplications.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobApplicationByIdAsync(entity.JobApplicationID, cancellationToken))!;
    }

    public async Task<JobApplicationReadDto> UpdateJobApplicationAsync(int id, JobApplicationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobApplications.FirstOrDefaultAsync(a => a.JobApplicationID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job application {id} was not found.");
        if (entity.Status == JobApplicationStatus.ConvertedToEmployee)
            throw new InvalidOperationException("Cannot update an application that was converted to an employee profile.");

        if (dto.ApplicantFirstName != null) entity.ApplicantFirstName = dto.ApplicantFirstName.Trim();
        if (dto.ApplicantLastName != null) entity.ApplicantLastName = dto.ApplicantLastName.Trim();
        if (dto.ApplicantArabicName != null) entity.ApplicantArabicName = dto.ApplicantArabicName.Trim();
        if (dto.ApplicantEnglishName != null) entity.ApplicantEnglishName = dto.ApplicantEnglishName.Trim();
        if (dto.NationalID != null) entity.NationalID = dto.NationalID.Trim();
        if (dto.DateOfBirth.HasValue) entity.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) entity.Gender = dto.Gender;
        if (dto.Phone != null) entity.Phone = dto.Phone;
        if (dto.Email != null) entity.Email = dto.Email.Trim();
        if (dto.Address != null) entity.Address = dto.Address;
        if (dto.HighestQualification != null) entity.HighestQualification = dto.HighestQualification;
        if (dto.Specialization != null) entity.Specialization = dto.Specialization;
        if (dto.YearsOfExperience.HasValue) entity.YearsOfExperience = dto.YearsOfExperience;
        if (dto.CurrentEmployer != null) entity.CurrentEmployer = dto.CurrentEmployer;
        if (dto.ResumeFileUrl != null) entity.ResumeFileUrl = dto.ResumeFileUrl;
        if (dto.CoverLetter != null) entity.CoverLetter = dto.CoverLetter;
        if (dto.Source != null) entity.Source = dto.Source;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobApplicationByIdAsync(id, cancellationToken))!;
    }

    public async Task<IReadOnlyList<JobApplicationListDto>> GetJobApplicationsAsync(JobApplicationFilterDto? filter, CancellationToken cancellationToken = default)
    {
        var q = _db.JobApplications.AsNoTracking().Include(a => a.JobPosting).AsQueryable();
        if (filter?.SchoolID is > 0)
            q = q.Where(a => a.SchoolID == filter.SchoolID);
        if (filter?.AcademicYearID is > 0)
            q = q.Where(a => a.AcademicYearID == filter.AcademicYearID);
        if (filter?.JobPostingID is > 0)
            q = q.Where(a => a.JobPostingID == filter.JobPostingID);
        if (filter?.Status is { } st)
            q = q.Where(a => a.Status == st);
        if (!string.IsNullOrWhiteSpace(filter?.Email))
            q = q.Where(a => a.Email == filter!.Email!.Trim());
        if (!string.IsNullOrWhiteSpace(filter?.NationalID))
            q = q.Where(a => a.NationalID == filter!.NationalID!.Trim());
        if (filter?.IsActive is bool ia)
            q = q.Where(a => a.IsActive == ia);

        return await q.OrderByDescending(a => a.AppliedAt)
            .Select(a => new JobApplicationListDto
            {
                JobApplicationID = a.JobApplicationID,
                JobPostingID = a.JobPostingID,
                JobPostingTitle = a.JobPosting.Title,
                SchoolID = a.SchoolID,
                ApplicantFirstName = a.ApplicantFirstName,
                ApplicantLastName = a.ApplicantLastName,
                Email = a.Email,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                ConvertedEmployeeProfileID = a.ConvertedEmployeeProfileID
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<JobApplicationReadDto?> GetJobApplicationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var a = await _db.JobApplications.AsNoTracking()
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.JobApplicationID == id, cancellationToken);
        if (a == null) return null;
        return MapApplicationRead(a);
    }

    private static JobApplicationReadDto MapApplicationRead(JobApplication a) => new()
    {
        JobApplicationID = a.JobApplicationID,
        JobPostingID = a.JobPostingID,
        JobPostingTitle = a.JobPosting.Title,
        SchoolID = a.SchoolID,
        AcademicYearID = a.AcademicYearID,
        ApplicantFirstName = a.ApplicantFirstName,
        ApplicantLastName = a.ApplicantLastName,
        ApplicantArabicName = a.ApplicantArabicName,
        ApplicantEnglishName = a.ApplicantEnglishName,
        NationalID = a.NationalID,
        DateOfBirth = a.DateOfBirth,
        Gender = a.Gender,
        Phone = a.Phone,
        Email = a.Email,
        Address = a.Address,
        HighestQualification = a.HighestQualification,
        Specialization = a.Specialization,
        YearsOfExperience = a.YearsOfExperience,
        CurrentEmployer = a.CurrentEmployer,
        ResumeFileUrl = a.ResumeFileUrl,
        CoverLetter = a.CoverLetter,
        Source = a.Source,
        Status = a.Status,
        AppliedAt = a.AppliedAt,
        Notes = a.Notes,
        ConvertedEmployeeProfileID = a.ConvertedEmployeeProfileID,
        IsActive = a.IsActive,
        CreatedAtUtc = a.CreatedAtUtc,
        UpdatedAtUtc = a.UpdatedAtUtc
    };

    public async Task<JobApplicationFullDto> GetJobApplicationFullAsync(int id, CancellationToken cancellationToken = default)
    {
        var a = await _db.JobApplications.AsNoTracking()
            .Include(x => x.JobPosting).ThenInclude(p => p!.JobType)
            .FirstOrDefaultAsync(x => x.JobApplicationID == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job application {id} was not found.");

        var postingDto = await GetJobPostingByIdAsync(a.JobPostingID, cancellationToken);

        var interviews = await _db.RecruitmentInterviews.AsNoTracking()
            .Where(i => i.JobApplicationID == id)
            .OrderBy(i => i.InterviewDate)
            .Select(i => new InterviewReadDto
            {
                InterviewID = i.InterviewID,
                JobApplicationID = i.JobApplicationID,
                SchoolID = i.SchoolID,
                AcademicYearID = i.AcademicYearID,
                InterviewDate = i.InterviewDate,
                InterviewType = i.InterviewType,
                LocationOrMeetingLink = i.LocationOrMeetingLink,
                InterviewerName = i.InterviewerName,
                InterviewerUserID = i.InterviewerUserID,
                InterviewerEmployeeProfileID = i.InterviewerEmployeeProfileID,
                Status = i.Status,
                Summary = i.Summary,
                Notes = i.Notes,
                Score = i.Score,
                CreatedAtUtc = i.CreatedAtUtc,
                UpdatedAtUtc = i.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var evals = await _db.CandidateEvaluations.AsNoTracking()
            .Where(e => e.JobApplicationID == id)
            .OrderByDescending(e => e.EvaluatedAt)
            .Select(e => new CandidateEvaluationReadDto
            {
                CandidateEvaluationID = e.CandidateEvaluationID,
                JobApplicationID = e.JobApplicationID,
                InterviewID = e.InterviewID,
                SchoolID = e.SchoolID,
                AcademicYearID = e.AcademicYearID,
                EvaluatorUserID = e.EvaluatorUserID,
                EvaluatorEmployeeProfileID = e.EvaluatorEmployeeProfileID,
                TechnicalScore = e.TechnicalScore,
                CommunicationScore = e.CommunicationScore,
                ClassManagementScore = e.ClassManagementScore,
                CultureFitScore = e.CultureFitScore,
                OverallScore = e.OverallScore,
                Strengths = e.Strengths,
                Weaknesses = e.Weaknesses,
                Recommendation = e.Recommendation,
                Notes = e.Notes,
                EvaluatedAt = e.EvaluatedAt,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var dec = await _db.HiringDecisions.AsNoTracking()
            .Include(d => d.OfferJobType)
            .FirstOrDefaultAsync(d => d.JobApplicationID == id, cancellationToken);

        HiringDecisionReadDto? decDto = null;
        if (dec != null)
        {
            decDto = new HiringDecisionReadDto
            {
                HiringDecisionID = dec.HiringDecisionID,
                JobApplicationID = dec.JobApplicationID,
                SchoolID = dec.SchoolID,
                AcademicYearID = dec.AcademicYearID,
                DecisionStatus = dec.DecisionStatus,
                DecisionDate = dec.DecisionDate,
                DecidedByUserID = dec.DecidedByUserID,
                DecidedByEmployeeProfileID = dec.DecidedByEmployeeProfileID,
                OfferJobTypeID = dec.OfferJobTypeID,
                OfferJobTypeName = dec.OfferJobType.Name,
                ProposedHireDate = dec.ProposedHireDate,
                ProposedSalaryNotes = dec.ProposedSalaryNotes,
                Reason = dec.Reason,
                InternalNotes = dec.InternalNotes,
                ConvertedEmployeeProfileID = dec.ConvertedEmployeeProfileID,
                CreatedAtUtc = dec.CreatedAtUtc,
                UpdatedAtUtc = dec.UpdatedAtUtc
            };
        }

        return new JobApplicationFullDto
        {
            Application = MapApplicationRead(a),
            Posting = postingDto,
            Interviews = interviews,
            Evaluations = evals,
            Decision = decDto
        };
    }

    private static void ValidateStatusTransition(JobApplicationStatus current, JobApplicationStatus next)
    {
        if (current == next) return;
        if (IsTerminalApplicationStatus(current))
            throw new InvalidOperationException($"Cannot change status from terminal state {current}.");

        var ok = (current, next) switch
        {
            (JobApplicationStatus.Submitted, JobApplicationStatus.UnderReview) => true,
            (JobApplicationStatus.Submitted, JobApplicationStatus.Withdrawn) => true,
            (JobApplicationStatus.UnderReview, JobApplicationStatus.InterviewScheduled) => true,
            (JobApplicationStatus.UnderReview, JobApplicationStatus.Rejected) => true,
            (JobApplicationStatus.UnderReview, JobApplicationStatus.Withdrawn) => true,
            (JobApplicationStatus.InterviewScheduled, JobApplicationStatus.Evaluated) => true,
            (JobApplicationStatus.InterviewScheduled, JobApplicationStatus.Rejected) => true,
            (JobApplicationStatus.InterviewScheduled, JobApplicationStatus.Withdrawn) => true,
            (JobApplicationStatus.Evaluated, JobApplicationStatus.Rejected) => true,
            (JobApplicationStatus.Evaluated, JobApplicationStatus.Withdrawn) => true,
            _ => false
        };
        if (!ok)
            throw new InvalidOperationException($"Invalid status transition from {current} to {next}.");
    }

    public async Task<JobApplicationReadDto> MoveJobApplicationStatusAsync(int id, JobApplicationStatusMoveDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobApplications.FirstOrDefaultAsync(a => a.JobApplicationID == id, cancellationToken)
                     ?? throw new KeyNotFoundException($"Job application {id} was not found.");
        ValidateStatusTransition(entity.Status, dto.NewStatus);
        entity.Status = dto.NewStatus;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetJobApplicationByIdAsync(id, cancellationToken))!;
    }

    public async Task<InterviewReadDto> ScheduleInterviewAsync(int jobApplicationId, InterviewCreateDto dto, CancellationToken cancellationToken = default)
    {
        var app = await _db.JobApplications
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");
        if (BlocksInterviews(app.Status))
            throw new InvalidOperationException("Cannot schedule an interview for a rejected or withdrawn application.");

        var interview = new Interview
        {
            JobApplicationID = app.JobApplicationID,
            SchoolID = app.SchoolID,
            AcademicYearID = app.AcademicYearID,
            InterviewDate = dto.InterviewDate,
            InterviewType = dto.InterviewType,
            LocationOrMeetingLink = dto.LocationOrMeetingLink,
            InterviewerName = dto.InterviewerName,
            InterviewerUserID = dto.InterviewerUserID,
            InterviewerEmployeeProfileID = dto.InterviewerEmployeeProfileID,
            Status = InterviewStatus.Scheduled,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.RecruitmentInterviews.Add(interview);

        if (app.Status is JobApplicationStatus.Submitted or JobApplicationStatus.UnderReview)
        {
            app.Status = JobApplicationStatus.InterviewScheduled;
            app.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (await MapInterviewReadAsync(interview.InterviewID, cancellationToken))!;
    }

    private async Task<InterviewReadDto?> MapInterviewReadAsync(int interviewId, CancellationToken cancellationToken)
    {
        var i = await _db.RecruitmentInterviews.AsNoTracking()
            .FirstOrDefaultAsync(x => x.InterviewID == interviewId, cancellationToken);
        if (i == null) return null;
        return new InterviewReadDto
        {
            InterviewID = i.InterviewID,
            JobApplicationID = i.JobApplicationID,
            SchoolID = i.SchoolID,
            AcademicYearID = i.AcademicYearID,
            InterviewDate = i.InterviewDate,
            InterviewType = i.InterviewType,
            LocationOrMeetingLink = i.LocationOrMeetingLink,
            InterviewerName = i.InterviewerName,
            InterviewerUserID = i.InterviewerUserID,
            InterviewerEmployeeProfileID = i.InterviewerEmployeeProfileID,
            Status = i.Status,
            Summary = i.Summary,
            Notes = i.Notes,
            Score = i.Score,
            CreatedAtUtc = i.CreatedAtUtc,
            UpdatedAtUtc = i.UpdatedAtUtc
        };
    }

    public async Task<InterviewReadDto> UpdateInterviewAsync(int interviewId, InterviewUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.RecruitmentInterviews.FirstOrDefaultAsync(i => i.InterviewID == interviewId, cancellationToken)
                     ?? throw new KeyNotFoundException($"Interview {interviewId} was not found.");

        if (dto.InterviewDate.HasValue) entity.InterviewDate = dto.InterviewDate.Value;
        if (dto.InterviewType != null) entity.InterviewType = dto.InterviewType;
        if (dto.LocationOrMeetingLink != null) entity.LocationOrMeetingLink = dto.LocationOrMeetingLink;
        if (dto.InterviewerName != null) entity.InterviewerName = dto.InterviewerName;
        if (dto.InterviewerUserID != null) entity.InterviewerUserID = dto.InterviewerUserID;
        if (dto.InterviewerEmployeeProfileID.HasValue) entity.InterviewerEmployeeProfileID = dto.InterviewerEmployeeProfileID;
        if (dto.Status.HasValue)
        {
            if (entity.Status == InterviewStatus.Completed && dto.Status == InterviewStatus.Scheduled)
                throw new InvalidOperationException("A completed interview cannot be set back to scheduled.");
            entity.Status = dto.Status.Value;
        }
        if (dto.Summary != null) entity.Summary = dto.Summary;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.Score.HasValue) entity.Score = dto.Score;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await MapInterviewReadAsync(interviewId, cancellationToken))!;
    }

    public async Task<IReadOnlyList<InterviewReadDto>> GetInterviewsForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default)
    {
        if (!await _db.JobApplications.AsNoTracking().AnyAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken))
            throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");

        return await _db.RecruitmentInterviews.AsNoTracking()
            .Where(i => i.JobApplicationID == jobApplicationId)
            .OrderBy(i => i.InterviewDate)
            .Select(i => new InterviewReadDto
            {
                InterviewID = i.InterviewID,
                JobApplicationID = i.JobApplicationID,
                SchoolID = i.SchoolID,
                AcademicYearID = i.AcademicYearID,
                InterviewDate = i.InterviewDate,
                InterviewType = i.InterviewType,
                LocationOrMeetingLink = i.LocationOrMeetingLink,
                InterviewerName = i.InterviewerName,
                InterviewerUserID = i.InterviewerUserID,
                InterviewerEmployeeProfileID = i.InterviewerEmployeeProfileID,
                Status = i.Status,
                Summary = i.Summary,
                Notes = i.Notes,
                Score = i.Score,
                CreatedAtUtc = i.CreatedAtUtc,
                UpdatedAtUtc = i.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public Task<InterviewReadDto> CompleteInterviewAsync(int interviewId, CancellationToken cancellationToken = default) =>
        SetInterviewStatusAsync(interviewId, InterviewStatus.Completed, cancellationToken);

    public Task<InterviewReadDto> CancelInterviewAsync(int interviewId, CancellationToken cancellationToken = default) =>
        SetInterviewStatusAsync(interviewId, InterviewStatus.Cancelled, cancellationToken);

    public Task<InterviewReadDto> NoShowInterviewAsync(int interviewId, CancellationToken cancellationToken = default) =>
        SetInterviewStatusAsync(interviewId, InterviewStatus.NoShow, cancellationToken);

    private async Task<InterviewReadDto> SetInterviewStatusAsync(int interviewId, InterviewStatus status, CancellationToken cancellationToken)
    {
        var entity = await _db.RecruitmentInterviews.FirstOrDefaultAsync(i => i.InterviewID == interviewId, cancellationToken)
                     ?? throw new KeyNotFoundException($"Interview {interviewId} was not found.");
        entity.Status = status;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (await MapInterviewReadAsync(interviewId, cancellationToken))!;
    }

    private static decimal? ComputeOverallScore(CandidateEvaluationCreateDto dto)
    {
        var parts = new List<decimal>();
        if (dto.TechnicalScore.HasValue) parts.Add(dto.TechnicalScore.Value);
        if (dto.CommunicationScore.HasValue) parts.Add(dto.CommunicationScore.Value);
        if (dto.ClassManagementScore.HasValue) parts.Add(dto.ClassManagementScore.Value);
        if (dto.CultureFitScore.HasValue) parts.Add(dto.CultureFitScore.Value);
        if (parts.Count == 0) return null;
        return Math.Round(parts.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static decimal? ComputeOverallScoreUpdate(CandidateEvaluationUpdateDto dto, CandidateEvaluation existing)
    {
        decimal? t = dto.TechnicalScore ?? existing.TechnicalScore;
        decimal? c = dto.CommunicationScore ?? existing.CommunicationScore;
        decimal? m = dto.ClassManagementScore ?? existing.ClassManagementScore;
        decimal? f = dto.CultureFitScore ?? existing.CultureFitScore;
        var parts = new List<decimal>();
        if (t.HasValue) parts.Add(t.Value);
        if (c.HasValue) parts.Add(c.Value);
        if (m.HasValue) parts.Add(m.Value);
        if (f.HasValue) parts.Add(f.Value);
        if (parts.Count == 0) return existing.OverallScore;
        return Math.Round(parts.Average(), 2, MidpointRounding.AwayFromZero);
    }

    public async Task<CandidateEvaluationReadDto> AddEvaluationAsync(int jobApplicationId, CandidateEvaluationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var app = await _db.JobApplications.FirstOrDefaultAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken)
                  ?? throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");
        if (BlocksInterviews(app.Status))
            throw new InvalidOperationException("Cannot add evaluations for a rejected or withdrawn application.");

        if (dto.InterviewID is int iid && iid > 0)
        {
            var interviewRow = await _db.RecruitmentInterviews.FirstOrDefaultAsync(x => x.InterviewID == iid, cancellationToken)
                     ?? throw new KeyNotFoundException($"Interview {iid} was not found.");
            if (interviewRow.JobApplicationID != jobApplicationId)
                throw new ArgumentException("Interview does not belong to this application.");
        }

        var overall = dto.OverallScore ?? ComputeOverallScore(dto);

        var entity = new CandidateEvaluation
        {
            JobApplicationID = app.JobApplicationID,
            InterviewID = dto.InterviewID is int linkId && linkId > 0 ? linkId : null,
            SchoolID = app.SchoolID,
            AcademicYearID = app.AcademicYearID,
            EvaluatorUserID = dto.EvaluatorUserID,
            EvaluatorEmployeeProfileID = dto.EvaluatorEmployeeProfileID,
            TechnicalScore = dto.TechnicalScore,
            CommunicationScore = dto.CommunicationScore,
            ClassManagementScore = dto.ClassManagementScore,
            CultureFitScore = dto.CultureFitScore,
            OverallScore = overall,
            Strengths = dto.Strengths,
            Weaknesses = dto.Weaknesses,
            Recommendation = dto.Recommendation,
            Notes = dto.Notes,
            EvaluatedAt = dto.EvaluatedAt ?? DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.CandidateEvaluations.Add(entity);

        if (app.Status is JobApplicationStatus.Submitted or JobApplicationStatus.UnderReview
            or JobApplicationStatus.InterviewScheduled)
        {
            app.Status = JobApplicationStatus.Evaluated;
            app.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (await MapEvaluationReadAsync(entity.CandidateEvaluationID, cancellationToken))!;
    }

    public async Task<CandidateEvaluationReadDto> UpdateEvaluationAsync(int evaluationId, CandidateEvaluationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CandidateEvaluations
            .Include(e => e.JobApplication)
            .FirstOrDefaultAsync(e => e.CandidateEvaluationID == evaluationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Evaluation {evaluationId} was not found.");

        if (dto.InterviewID.HasValue)
        {
            var newIv = dto.InterviewID.Value;
            if (newIv > 0)
            {
                var iv = await _db.RecruitmentInterviews.FirstOrDefaultAsync(x => x.InterviewID == newIv, cancellationToken)
                         ?? throw new KeyNotFoundException($"Interview {newIv} was not found.");
                if (iv.JobApplicationID != entity.JobApplicationID)
                    throw new ArgumentException("Interview does not belong to this application.");
            }
            entity.InterviewID = newIv == 0 ? null : newIv;
        }

        if (dto.EvaluatorUserID != null) entity.EvaluatorUserID = dto.EvaluatorUserID;
        if (dto.EvaluatorEmployeeProfileID.HasValue) entity.EvaluatorEmployeeProfileID = dto.EvaluatorEmployeeProfileID;
        if (dto.TechnicalScore.HasValue) entity.TechnicalScore = dto.TechnicalScore;
        if (dto.CommunicationScore.HasValue) entity.CommunicationScore = dto.CommunicationScore;
        if (dto.ClassManagementScore.HasValue) entity.ClassManagementScore = dto.ClassManagementScore;
        if (dto.CultureFitScore.HasValue) entity.CultureFitScore = dto.CultureFitScore;
        if (dto.OverallScore.HasValue)
            entity.OverallScore = dto.OverallScore;
        else
            entity.OverallScore = ComputeOverallScoreUpdate(dto, entity);
        if (dto.Strengths != null) entity.Strengths = dto.Strengths;
        if (dto.Weaknesses != null) entity.Weaknesses = dto.Weaknesses;
        if (dto.Recommendation.HasValue) entity.Recommendation = dto.Recommendation.Value;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.EvaluatedAt.HasValue) entity.EvaluatedAt = dto.EvaluatedAt.Value;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return (await MapEvaluationReadAsync(evaluationId, cancellationToken))!;
    }

    private async Task<CandidateEvaluationReadDto?> MapEvaluationReadAsync(int id, CancellationToken cancellationToken)
    {
        var e = await _db.CandidateEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CandidateEvaluationID == id, cancellationToken);
        if (e == null) return null;
        return new CandidateEvaluationReadDto
        {
            CandidateEvaluationID = e.CandidateEvaluationID,
            JobApplicationID = e.JobApplicationID,
            InterviewID = e.InterviewID,
            SchoolID = e.SchoolID,
            AcademicYearID = e.AcademicYearID,
            EvaluatorUserID = e.EvaluatorUserID,
            EvaluatorEmployeeProfileID = e.EvaluatorEmployeeProfileID,
            TechnicalScore = e.TechnicalScore,
            CommunicationScore = e.CommunicationScore,
            ClassManagementScore = e.ClassManagementScore,
            CultureFitScore = e.CultureFitScore,
            OverallScore = e.OverallScore,
            Strengths = e.Strengths,
            Weaknesses = e.Weaknesses,
            Recommendation = e.Recommendation,
            Notes = e.Notes,
            EvaluatedAt = e.EvaluatedAt,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc
        };
    }

    public async Task<IReadOnlyList<CandidateEvaluationReadDto>> GetEvaluationsForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default)
    {
        if (!await _db.JobApplications.AsNoTracking().AnyAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken))
            throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");

        return await _db.CandidateEvaluations.AsNoTracking()
            .Where(e => e.JobApplicationID == jobApplicationId)
            .OrderByDescending(e => e.EvaluatedAt)
            .Select(e => new CandidateEvaluationReadDto
            {
                CandidateEvaluationID = e.CandidateEvaluationID,
                JobApplicationID = e.JobApplicationID,
                InterviewID = e.InterviewID,
                SchoolID = e.SchoolID,
                AcademicYearID = e.AcademicYearID,
                EvaluatorUserID = e.EvaluatorUserID,
                EvaluatorEmployeeProfileID = e.EvaluatorEmployeeProfileID,
                TechnicalScore = e.TechnicalScore,
                CommunicationScore = e.CommunicationScore,
                ClassManagementScore = e.ClassManagementScore,
                CultureFitScore = e.CultureFitScore,
                OverallScore = e.OverallScore,
                Strengths = e.Strengths,
                Weaknesses = e.Weaknesses,
                Recommendation = e.Recommendation,
                Notes = e.Notes,
                EvaluatedAt = e.EvaluatedAt,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedAtUtc = e.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HiringDecisionReadDto?> GetDecisionForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default)
    {
        var dec = await _db.HiringDecisions.AsNoTracking()
            .Include(d => d.OfferJobType)
            .FirstOrDefaultAsync(d => d.JobApplicationID == jobApplicationId, cancellationToken);
        if (dec == null) return null;
        return new HiringDecisionReadDto
        {
            HiringDecisionID = dec.HiringDecisionID,
            JobApplicationID = dec.JobApplicationID,
            SchoolID = dec.SchoolID,
            AcademicYearID = dec.AcademicYearID,
            DecisionStatus = dec.DecisionStatus,
            DecisionDate = dec.DecisionDate,
            DecidedByUserID = dec.DecidedByUserID,
            DecidedByEmployeeProfileID = dec.DecidedByEmployeeProfileID,
            OfferJobTypeID = dec.OfferJobTypeID,
            OfferJobTypeName = dec.OfferJobType.Name,
            ProposedHireDate = dec.ProposedHireDate,
            ProposedSalaryNotes = dec.ProposedSalaryNotes,
            Reason = dec.Reason,
            InternalNotes = dec.InternalNotes,
            ConvertedEmployeeProfileID = dec.ConvertedEmployeeProfileID,
            CreatedAtUtc = dec.CreatedAtUtc,
            UpdatedAtUtc = dec.UpdatedAtUtc
        };
    }

    private async Task EnsureEvaluationGateAsync(int jobApplicationId, bool skipCheck, CancellationToken cancellationToken)
    {
        if (skipCheck) return;
        var hasEval = await _db.CandidateEvaluations.AsNoTracking()
            .AnyAsync(e => e.JobApplicationID == jobApplicationId, cancellationToken);
        if (!hasEval)
            throw new InvalidOperationException("At least one candidate evaluation is required before recording a hiring decision (or set SkipEvaluationCheck).");
    }

    public async Task<HiringDecisionReadDto> CreateDecisionAsync(int jobApplicationId, HiringDecisionCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (await _db.HiringDecisions.AnyAsync(d => d.JobApplicationID == jobApplicationId, cancellationToken))
            throw new InvalidOperationException("A hiring decision already exists for this application. Use PUT to update it.");

        var app = await _db.JobApplications.FirstOrDefaultAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken)
                  ?? throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");
        if (IsTerminalApplicationStatus(app.Status))
            throw new InvalidOperationException("Cannot create a hiring decision for a terminal application status.");

        await GetJobTypeAsync(dto.OfferJobTypeID, cancellationToken);

        if (dto.DecisionStatus is HiringDecisionStatus.Accepted or HiringDecisionStatus.Rejected)
            await EnsureEvaluationGateAsync(jobApplicationId, dto.SkipEvaluationCheck, cancellationToken);

        var entity = new HiringDecision
        {
            JobApplicationID = app.JobApplicationID,
            SchoolID = app.SchoolID,
            AcademicYearID = app.AcademicYearID,
            DecisionStatus = dto.DecisionStatus,
            DecisionDate = dto.DecisionDate ?? DateTime.UtcNow,
            DecidedByUserID = dto.DecidedByUserID,
            DecidedByEmployeeProfileID = dto.DecidedByEmployeeProfileID,
            OfferJobTypeID = dto.OfferJobTypeID,
            ProposedHireDate = dto.ProposedHireDate,
            ProposedSalaryNotes = dto.ProposedSalaryNotes,
            Reason = dto.Reason,
            InternalNotes = dto.InternalNotes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.HiringDecisions.Add(entity);
        ApplyApplicationStatusFromDecision(app, dto.DecisionStatus);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetDecisionForApplicationAsync(jobApplicationId, cancellationToken))!;
    }

    private static void ApplyApplicationStatusFromDecision(JobApplication app, HiringDecisionStatus decisionStatus)
    {
        app.UpdatedAtUtc = DateTime.UtcNow;
        if (decisionStatus == HiringDecisionStatus.Accepted)
            app.Status = JobApplicationStatus.Accepted;
        else if (decisionStatus == HiringDecisionStatus.Rejected)
            app.Status = JobApplicationStatus.Rejected;
    }

    public async Task<HiringDecisionReadDto> UpdateDecisionAsync(int hiringDecisionId, HiringDecisionUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.HiringDecisions
            .Include(d => d.JobApplication)
            .FirstOrDefaultAsync(d => d.HiringDecisionID == hiringDecisionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hiring decision {hiringDecisionId} was not found.");

        if (entity.ConvertedEmployeeProfileID.HasValue && dto.DecisionStatus is HiringDecisionStatus.Rejected)
            throw new InvalidOperationException("Cannot reject a decision that was already converted to an employee profile.");

        if (dto.OfferJobTypeID.HasValue)
            await GetJobTypeAsync(dto.OfferJobTypeID.Value, cancellationToken);

        if (dto.DecisionStatus.HasValue) entity.DecisionStatus = dto.DecisionStatus.Value;
        if (dto.DecisionDate.HasValue) entity.DecisionDate = dto.DecisionDate.Value;
        if (dto.DecidedByUserID != null) entity.DecidedByUserID = dto.DecidedByUserID;
        if (dto.DecidedByEmployeeProfileID.HasValue) entity.DecidedByEmployeeProfileID = dto.DecidedByEmployeeProfileID;
        if (dto.OfferJobTypeID.HasValue) entity.OfferJobTypeID = dto.OfferJobTypeID.Value;
        if (dto.ProposedHireDate.HasValue) entity.ProposedHireDate = dto.ProposedHireDate;
        if (dto.ProposedSalaryNotes != null) entity.ProposedSalaryNotes = dto.ProposedSalaryNotes;
        if (dto.Reason != null) entity.Reason = dto.Reason;
        if (dto.InternalNotes != null) entity.InternalNotes = dto.InternalNotes;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (dto.DecisionStatus.HasValue)
            ApplyApplicationStatusFromDecision(entity.JobApplication, dto.DecisionStatus.Value);

        await _db.SaveChangesAsync(cancellationToken);
        return (await GetDecisionForApplicationAsync(entity.JobApplicationID, cancellationToken))!;
    }

    public async Task<HiringDecisionReadDto> AcceptApplicationAsync(int jobApplicationId, HiringDecisionCreateDto dto, string? decidedByUserId, CancellationToken cancellationToken = default)
    {
        dto.DecisionStatus = HiringDecisionStatus.Accepted;
        if (string.IsNullOrWhiteSpace(dto.DecidedByUserID) && !string.IsNullOrWhiteSpace(decidedByUserId))
            dto.DecidedByUserID = decidedByUserId;

        var existing = await _db.HiringDecisions.Include(d => d.JobApplication)
            .FirstOrDefaultAsync(d => d.JobApplicationID == jobApplicationId, cancellationToken);
        if (existing != null)
        {
            await GetJobTypeAsync(dto.OfferJobTypeID, cancellationToken);
            await EnsureEvaluationGateAsync(jobApplicationId, dto.SkipEvaluationCheck, cancellationToken);
            existing.DecisionStatus = HiringDecisionStatus.Accepted;
            existing.DecisionDate = dto.DecisionDate ?? DateTime.UtcNow;
            existing.DecidedByUserID = dto.DecidedByUserID ?? existing.DecidedByUserID;
            existing.DecidedByEmployeeProfileID = dto.DecidedByEmployeeProfileID ?? existing.DecidedByEmployeeProfileID;
            existing.OfferJobTypeID = dto.OfferJobTypeID;
            existing.ProposedHireDate = dto.ProposedHireDate;
            existing.ProposedSalaryNotes = dto.ProposedSalaryNotes ?? existing.ProposedSalaryNotes;
            existing.Reason = dto.Reason ?? existing.Reason;
            existing.InternalNotes = dto.InternalNotes ?? existing.InternalNotes;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            ApplyApplicationStatusFromDecision(existing.JobApplication, HiringDecisionStatus.Accepted);
            await _db.SaveChangesAsync(cancellationToken);
            return (await GetDecisionForApplicationAsync(jobApplicationId, cancellationToken))!;
        }

        return await CreateDecisionAsync(jobApplicationId, dto, cancellationToken);
    }

    public async Task<HiringDecisionReadDto> RejectApplicationAsync(int jobApplicationId, HiringDecisionCreateDto dto, string? decidedByUserId, CancellationToken cancellationToken = default)
    {
        dto.DecisionStatus = HiringDecisionStatus.Rejected;
        if (string.IsNullOrWhiteSpace(dto.DecidedByUserID) && !string.IsNullOrWhiteSpace(decidedByUserId))
            dto.DecidedByUserID = decidedByUserId;

        var existing = await _db.HiringDecisions.Include(d => d.JobApplication)
            .FirstOrDefaultAsync(d => d.JobApplicationID == jobApplicationId, cancellationToken);
        if (existing != null)
        {
            if (existing.ConvertedEmployeeProfileID.HasValue)
                throw new InvalidOperationException("Cannot reject after conversion to employee.");
            await EnsureEvaluationGateAsync(jobApplicationId, dto.SkipEvaluationCheck, cancellationToken);
            existing.DecisionStatus = HiringDecisionStatus.Rejected;
            existing.DecisionDate = dto.DecisionDate ?? DateTime.UtcNow;
            existing.DecidedByUserID = dto.DecidedByUserID ?? existing.DecidedByUserID;
            existing.OfferJobTypeID = dto.OfferJobTypeID;
            existing.Reason = dto.Reason ?? existing.Reason;
            existing.InternalNotes = dto.InternalNotes ?? existing.InternalNotes;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            ApplyApplicationStatusFromDecision(existing.JobApplication, HiringDecisionStatus.Rejected);
            await _db.SaveChangesAsync(cancellationToken);
            return (await GetDecisionForApplicationAsync(jobApplicationId, cancellationToken))!;
        }

        return await CreateDecisionAsync(jobApplicationId, dto, cancellationToken);
    }

    public async Task<ConvertApplicantToEmployeeResultDto> ConvertAcceptedApplicantToEmployeeAsync(int jobApplicationId, ConvertApplicantToEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var app = await _db.JobApplications
            .Include(a => a.JobPosting).ThenInclude(p => p!.JobType)
            .FirstOrDefaultAsync(a => a.JobApplicationID == jobApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job application {jobApplicationId} was not found.");

        if (app.ConvertedEmployeeProfileID.HasValue)
            throw new InvalidOperationException("This application was already converted to an employee profile.");

        var decision = await _db.HiringDecisions
            .FirstOrDefaultAsync(d => d.JobApplicationID == jobApplicationId, cancellationToken)
            ?? throw new InvalidOperationException("A hiring decision is required before conversion.");

        if (decision.DecisionStatus != HiringDecisionStatus.Accepted || app.Status != JobApplicationStatus.Accepted)
            throw new InvalidOperationException("Only accepted applications can be converted to an employee profile.");

        var jobTypeId = dto.EmployeeJobTypeID ?? decision.OfferJobTypeID;
        await GetJobTypeAsync(jobTypeId, cancellationToken);

        var code = string.IsNullOrWhiteSpace(dto.EmployeeCode)
            ? $"HIRE-{jobApplicationId}"
            : dto.EmployeeCode!.Trim();

        var fullName = new EmployeeNameDto
        {
            FirstName = app.ApplicantFirstName,
            LastName = app.ApplicantLastName,
            MiddleName = null
        };

        EmployeeNameAlisDto? alis = null;
        if (!string.IsNullOrWhiteSpace(app.ApplicantEnglishName) || !string.IsNullOrWhiteSpace(app.ApplicantArabicName))
        {
            alis = new EmployeeNameAlisDto
            {
                FirstNameEng = app.ApplicantEnglishName,
                MiddleNameEng = null,
                LastNameEng = null
            };
        }

        var create = new EmployeeProfileCreateDto
        {
            UserId = dto.UserId,
            SchoolID = app.SchoolID,
            CurrentAcademicYearID = app.AcademicYearID,
            EmployeeJobTypeID = jobTypeId,
            EmployeeCode = code,
            FullName = fullName,
            FullNameAlis = alis,
            NationalId = app.NationalID,
            DateOfBirth = app.DateOfBirth,
            Gender = app.Gender,
            Phone = app.Phone,
            Email = app.Email,
            Address = app.Address,
            HireDate = dto.HireDate ?? decision.ProposedHireDate,
            EmploymentStatus = dto.EmploymentStatus,
            Notes = dto.Notes,
            IsActive = true
        };

        var profile = await _employees.CreateAsync(create, cancellationToken);

        if (dto.MapQualificationAndSpecialization)
        {
            if (!string.IsNullOrWhiteSpace(app.HighestQualification))
            {
                await _employees.AddQualificationAsync(profile.EmployeeProfileID, new EmployeeQualificationDto
                {
                    DegreeName = app.HighestQualification.Trim(),
                    Major = app.Specialization,
                    Notes = "Imported from recruitment application."
                }, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(app.Specialization))
            {
                await _employees.AddSpecializationAsync(profile.EmployeeProfileID, new EmployeeSpecializationDto
                {
                    Name = app.Specialization.Trim(),
                    Notes = "Imported from recruitment application."
                }, cancellationToken);
            }
        }

        app.ConvertedEmployeeProfileID = profile.EmployeeProfileID;
        app.Status = JobApplicationStatus.ConvertedToEmployee;
        app.UpdatedAtUtc = DateTime.UtcNow;

        decision.ConvertedEmployeeProfileID = profile.EmployeeProfileID;
        decision.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new ConvertApplicantToEmployeeResultDto
        {
            EmployeeProfileID = profile.EmployeeProfileID,
            EmployeeCode = profile.EmployeeCode,
            JobApplicationID = jobApplicationId
        };
    }
}
