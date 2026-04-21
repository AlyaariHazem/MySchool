using Backend.Data;
using Backend.DTOS.School.SupervisorVisit;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class SupervisorVisitRepository : ISupervisorVisitRepository
{
    private readonly TenantDbContext _db;

    public SupervisorVisitRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatPersonName(Name? n)
    {
        if (n == null) return string.Empty;
        return string.Join(" ", new[] { n.FirstName, n.MiddleName, n.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    public async Task<IReadOnlyList<SupervisorVisitListItemDto>> ListAsync(SupervisorVisitFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter ??= new SupervisorVisitFilterDto();
        var q = _db.SupervisorVisits.AsNoTracking().AsQueryable();

        if (filter.SchoolID is > 0)
            q = q.Where(v => v.SchoolID == filter.SchoolID);
        if (filter.AcademicYearID is > 0)
            q = q.Where(v => v.AcademicYearID == filter.AcademicYearID);
        if (filter.VisitedTeacherID is > 0)
            q = q.Where(v => v.VisitedTeacherID == filter.VisitedTeacherID);
        if (filter.FromDate is { } fd)
            q = q.Where(v => v.VisitDate >= fd);
        if (filter.ToDate is { } td)
            q = q.Where(v => v.VisitDate <= td);

        var raw = await q
            .OrderByDescending(v => v.VisitDate)
            .ThenByDescending(v => v.SupervisorVisitID)
            .Select(v => new
            {
                v.SupervisorVisitID,
                v.SchoolID,
                v.AcademicYearID,
                v.VisitedTeacherID,
                TFirst = v.VisitedTeacher.FullName.FirstName,
                TMid = v.VisitedTeacher.FullName.MiddleName,
                TLast = v.VisitedTeacher.FullName.LastName,
                v.ClassID,
                ClassName = v.Class != null ? v.Class.ClassName : null,
                v.SubjectID,
                SubjectName = v.Subject != null ? v.Subject.SubjectName : null,
                v.SupervisorEmployeeProfileID,
                SFirst = v.SupervisorEmployeeProfile.FullName.FirstName,
                SMid = v.SupervisorEmployeeProfile.FullName.MiddleName,
                SLast = v.SupervisorEmployeeProfile.FullName.LastName,
                v.VisitDate,
                v.Status,
                v.OverallScoreOutOf100,
            })
            .ToListAsync(cancellationToken);

        return raw.Select(v => new SupervisorVisitListItemDto
        {
            SupervisorVisitID = v.SupervisorVisitID,
            SchoolID = v.SchoolID,
            AcademicYearID = v.AcademicYearID,
            VisitedTeacherID = v.VisitedTeacherID,
            VisitedTeacherName = FormatPersonName(new Name { FirstName = v.TFirst, MiddleName = v.TMid, LastName = v.TLast }),
            ClassID = v.ClassID,
            ClassName = v.ClassName,
            SubjectID = v.SubjectID,
            SubjectName = v.SubjectName,
            SupervisorEmployeeProfileID = v.SupervisorEmployeeProfileID,
            SupervisorName = FormatPersonName(new Name { FirstName = v.SFirst, MiddleName = v.SMid, LastName = v.SLast }),
            VisitDate = v.VisitDate,
            Status = (int)v.Status,
            OverallScoreOutOf100 = v.OverallScoreOutOf100,
        }).ToList();
    }

    public async Task<SupervisorVisitDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var visit = await _db.SupervisorVisits.AsNoTracking()
            .Include(v => v.VisitedTeacher)
            .Include(v => v.Class)
            .Include(v => v.Subject)
            .Include(v => v.SupervisorEmployeeProfile)
            .Include(v => v.Observations)
            .Include(v => v.Recommendations)
            .ThenInclude(r => r.FollowUps)
            .FirstOrDefaultAsync(v => v.SupervisorVisitID == id, cancellationToken);

        if (visit == null) return null;

        return new SupervisorVisitDetailDto
        {
            SupervisorVisitID = visit.SupervisorVisitID,
            SchoolID = visit.SchoolID,
            AcademicYearID = visit.AcademicYearID,
            VisitedTeacherID = visit.VisitedTeacherID,
            VisitedTeacherName = FormatPersonName(visit.VisitedTeacher.FullName),
            ClassID = visit.ClassID,
            ClassName = visit.Class?.ClassName,
            SubjectID = visit.SubjectID,
            SubjectName = visit.Subject?.SubjectName,
            SupervisorEmployeeProfileID = visit.SupervisorEmployeeProfileID,
            SupervisorName = FormatPersonName(visit.SupervisorEmployeeProfile.FullName),
            VisitDate = visit.VisitDate,
            Status = (int)visit.Status,
            OverallScoreOutOf100 = visit.OverallScoreOutOf100,
            SummaryNotes = visit.SummaryNotes,
            CreatedAtUtc = visit.CreatedAtUtc,
            UpdatedAtUtc = visit.UpdatedAtUtc,
            Observations = visit.Observations
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.VisitObservationID)
                .Select(o => new VisitObservationReadDto
                {
                    VisitObservationID = o.VisitObservationID,
                    SupervisorVisitID = o.SupervisorVisitID,
                    Category = o.Category,
                    ObservationText = o.ObservationText,
                    SortOrder = o.SortOrder,
                    CreatedAtUtc = o.CreatedAtUtc,
                })
                .ToList(),
            Recommendations = visit.Recommendations
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.VisitRecommendationID)
                .Select(r => new VisitRecommendationReadDto
                {
                    VisitRecommendationID = r.VisitRecommendationID,
                    SupervisorVisitID = r.SupervisorVisitID,
                    RecommendationText = r.RecommendationText,
                    ImplementationStatus = (int)r.ImplementationStatus,
                    DueDate = r.DueDate,
                    CompletedAtUtc = r.CompletedAtUtc,
                    SortOrder = r.SortOrder,
                    CreatedAtUtc = r.CreatedAtUtc,
                    UpdatedAtUtc = r.UpdatedAtUtc,
                    FollowUps = r.FollowUps
                        .OrderBy(f => f.FollowUpDate)
                        .ThenBy(f => f.RecommendationFollowUpID)
                        .Select(f => new RecommendationFollowUpReadDto
                        {
                            RecommendationFollowUpID = f.RecommendationFollowUpID,
                            VisitRecommendationID = f.VisitRecommendationID,
                            FollowUpNote = f.FollowUpNote,
                            FollowUpDate = f.FollowUpDate,
                            FollowUpByEmployeeProfileID = f.FollowUpByEmployeeProfileID,
                            CreatedAtUtc = f.CreatedAtUtc,
                        })
                        .ToList(),
                })
                .ToList(),
        };
    }

    public async Task<int> CreateAsync(SupervisorVisitWriteDto dto, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var visit = new SupervisorVisit
        {
            SchoolID = dto.SchoolID,
            AcademicYearID = dto.AcademicYearID,
            VisitedTeacherID = dto.VisitedTeacherID,
            ClassID = dto.ClassID,
            SubjectID = dto.SubjectID,
            SupervisorEmployeeProfileID = dto.SupervisorEmployeeProfileID,
            VisitDate = dto.VisitDate,
            Status = (SupervisorVisitStatus)dto.Status,
            OverallScoreOutOf100 = dto.OverallScoreOutOf100,
            SummaryNotes = dto.SummaryNotes,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        foreach (var o in dto.Observations ?? Enumerable.Empty<VisitObservationWriteDto>())
        {
            visit.Observations.Add(new VisitObservation
            {
                Category = o.Category,
                ObservationText = o.ObservationText,
                SortOrder = o.SortOrder,
                CreatedAtUtc = now,
            });
        }

        foreach (var r in dto.Recommendations ?? Enumerable.Empty<VisitRecommendationWriteDto>())
        {
            var rec = new VisitRecommendation
            {
                RecommendationText = r.RecommendationText,
                ImplementationStatus = (RecommendationImplementationStatus)r.ImplementationStatus,
                DueDate = r.DueDate,
                CompletedAtUtc = r.CompletedAtUtc,
                SortOrder = r.SortOrder,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            foreach (var f in r.FollowUps ?? Enumerable.Empty<RecommendationFollowUpWriteDto>())
            {
                rec.FollowUps.Add(new RecommendationFollowUp
                {
                    FollowUpNote = f.FollowUpNote,
                    FollowUpDate = f.FollowUpDate,
                    FollowUpByEmployeeProfileID = f.FollowUpByEmployeeProfileID,
                    CreatedAtUtc = now,
                });
            }

            visit.Recommendations.Add(rec);
        }

        _db.SupervisorVisits.Add(visit);
        await _db.SaveChangesAsync(cancellationToken);
        return visit.SupervisorVisitID;
    }

    public async Task UpdateAsync(int id, SupervisorVisitWriteDto dto, CancellationToken cancellationToken = default)
    {
        var visit = await _db.SupervisorVisits
            .Include(v => v.Observations)
            .Include(v => v.Recommendations)
            .ThenInclude(r => r.FollowUps)
            .FirstOrDefaultAsync(v => v.SupervisorVisitID == id, cancellationToken)
            ?? throw new InvalidOperationException("Visit not found.");

        foreach (var r in visit.Recommendations.ToList())
        {
            _db.RecommendationFollowUps.RemoveRange(r.FollowUps);
        }

        _db.VisitRecommendations.RemoveRange(visit.Recommendations);
        _db.VisitObservations.RemoveRange(visit.Observations);
        visit.Observations.Clear();
        visit.Recommendations.Clear();
        await _db.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        visit.SchoolID = dto.SchoolID;
        visit.AcademicYearID = dto.AcademicYearID;
        visit.VisitedTeacherID = dto.VisitedTeacherID;
        visit.ClassID = dto.ClassID;
        visit.SubjectID = dto.SubjectID;
        visit.SupervisorEmployeeProfileID = dto.SupervisorEmployeeProfileID;
        visit.VisitDate = dto.VisitDate;
        visit.Status = (SupervisorVisitStatus)dto.Status;
        visit.OverallScoreOutOf100 = dto.OverallScoreOutOf100;
        visit.SummaryNotes = dto.SummaryNotes;
        visit.UpdatedAtUtc = now;

        foreach (var o in dto.Observations ?? Enumerable.Empty<VisitObservationWriteDto>())
        {
            visit.Observations.Add(new VisitObservation
            {
                Category = o.Category,
                ObservationText = o.ObservationText,
                SortOrder = o.SortOrder,
                CreatedAtUtc = now,
            });
        }

        foreach (var r in dto.Recommendations ?? Enumerable.Empty<VisitRecommendationWriteDto>())
        {
            var rec = new VisitRecommendation
            {
                RecommendationText = r.RecommendationText,
                ImplementationStatus = (RecommendationImplementationStatus)r.ImplementationStatus,
                DueDate = r.DueDate,
                CompletedAtUtc = r.CompletedAtUtc,
                SortOrder = r.SortOrder,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            foreach (var f in r.FollowUps ?? Enumerable.Empty<RecommendationFollowUpWriteDto>())
            {
                rec.FollowUps.Add(new RecommendationFollowUp
                {
                    FollowUpNote = f.FollowUpNote,
                    FollowUpDate = f.FollowUpDate,
                    FollowUpByEmployeeProfileID = f.FollowUpByEmployeeProfileID,
                    CreatedAtUtc = now,
                });
            }

            visit.Recommendations.Add(rec);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var visit = await _db.SupervisorVisits
            .Include(v => v.Observations)
            .Include(v => v.Recommendations)
            .ThenInclude(r => r.FollowUps)
            .FirstOrDefaultAsync(v => v.SupervisorVisitID == id, cancellationToken);
        if (visit == null) return false;

        foreach (var r in visit.Recommendations.ToList())
            _db.RecommendationFollowUps.RemoveRange(r.FollowUps);
        _db.VisitRecommendations.RemoveRange(visit.Recommendations);
        _db.VisitObservations.RemoveRange(visit.Observations);
        _db.SupervisorVisits.Remove(visit);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int?> GetSchoolIdForVisitAsync(int visitId, CancellationToken cancellationToken = default) =>
        _db.SupervisorVisits.AsNoTracking()
            .Where(v => v.SupervisorVisitID == visitId)
            .Select(v => (int?)v.SchoolID)
            .FirstOrDefaultAsync(cancellationToken);
}
