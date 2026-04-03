using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Attendance;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.School;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly TenantDbContext _db;

    public AttendanceRepository(TenantDbContext db)
    {
        _db = db;
    }

    private static string FormatStudentName(Student s) =>
        $"{s.FullName.FirstName} {(s.FullName.MiddleName ?? "").Trim()} {s.FullName.LastName}".Replace("  ", " ").Trim();

    private static AttendanceDTO MapToDto(Attendance a) => new()
    {
        AttendanceId = a.AttendanceId,
        StudentID = a.StudentID,
        StudentName = a.Student != null ? FormatStudentName(a.Student) : null,
        ClassID = a.ClassID,
        ClassName = a.Class?.ClassName,
        Date = a.AttendanceDate,
        Status = a.Status,
        Remarks = a.Remarks,
        RecordedBy = a.RecordedBy,
        TenantId = a.TenantId,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    private async Task EnsureStudentInClassAsync(int studentId, int classId)
    {
        var ok = await _db.Students
            .AsNoTracking()
            .Include(s => s.Division)
            .AnyAsync(s => s.StudentID == studentId && s.Division.ClassID == classId);

        if (!ok)
            throw new InvalidOperationException("Student is not enrolled in this class (division mismatch).");
    }

    private async Task EnsureNoDuplicateAsync(int studentId, int classId, DateOnly date, Guid? exceptAttendanceId = null)
    {
        var exists = await _db.Attendances
            .AnyAsync(a =>
                a.StudentID == studentId &&
                a.ClassID == classId &&
                a.AttendanceDate == date &&
                (!exceptAttendanceId.HasValue || a.AttendanceId != exceptAttendanceId.Value));

        if (exists)
            throw new InvalidOperationException("Attendance for this student, class, and date already exists.");
    }

    public async Task<AttendanceDTO?> GetByIdAsync(Guid id)
    {
        var a = await _db.Attendances
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.AttendanceId == id);

        return a == null ? null : MapToDto(a);
    }

    public async Task<List<AttendanceDTO>> GetByClassAndDateAsync(int classId, DateOnly date)
    {
        var list = await _db.Attendances
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .Where(x => x.ClassID == classId && x.AttendanceDate == date)
            .OrderBy(x => x.Student.FullName.FirstName)
            .ThenBy(x => x.Student.FullName.LastName)
            .ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task<List<AttendanceDTO>> GetByStudentAsync(int studentId, DateOnly? from = null, DateOnly? to = null)
    {
        var q = _db.Attendances
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .Where(x => x.StudentID == studentId);

        if (from.HasValue)
            q = q.Where(x => x.AttendanceDate >= from.Value);
        if (to.HasValue)
            q = q.Where(x => x.AttendanceDate <= to.Value);

        var list = await q
            .OrderByDescending(x => x.AttendanceDate)
            .ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task<AttendanceDTO> CreateAsync(CreateAttendanceDTO dto, string recordedByUserId, int? tenantId)
    {
        await EnsureStudentInClassAsync(dto.StudentID, dto.ClassID);
        await EnsureNoDuplicateAsync(dto.StudentID, dto.ClassID, dto.Date);

        var entity = new Attendance
        {
            AttendanceId = Guid.NewGuid(),
            StudentID = dto.StudentID,
            ClassID = dto.ClassID,
            AttendanceDate = dto.Date,
            Status = dto.Status,
            Remarks = dto.Remarks,
            RecordedBy = recordedByUserId,
            TenantId = tenantId,
            CreatedAt = DateTime.Now
        };

        _db.Attendances.Add(entity);
        await _db.SaveChangesAsync();

        var created = await _db.Attendances
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .FirstAsync(x => x.AttendanceId == entity.AttendanceId);

        return MapToDto(created);
    }

    public async Task<AttendanceDTO?> UpdateAsync(Guid id, UpdateAttendanceDTO dto)
    {
        var entity = await _db.Attendances.FirstOrDefaultAsync(x => x.AttendanceId == id);
        if (entity == null)
            return null;

        var newDate = dto.Date ?? entity.AttendanceDate;
        if (newDate != entity.AttendanceDate)
            await EnsureNoDuplicateAsync(entity.StudentID, entity.ClassID, newDate, id);

        entity.Status = dto.Status;
        entity.Remarks = dto.Remarks;
        entity.AttendanceDate = newDate;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        var updated = await _db.Attendances
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Class)
            .FirstAsync(x => x.AttendanceId == id);

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.Attendances.FirstOrDefaultAsync(x => x.AttendanceId == id);
        if (entity == null)
            return false;

        _db.Attendances.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkUpsertAsync(BulkAttendanceDTO dto, string recordedByUserId, int? tenantId)
    {
        if (dto.Entries == null || dto.Entries.Count == 0)
            throw new InvalidOperationException("Entries are required.");

        if (dto.Entries.Count != dto.Entries.Select(e => e.StudentID).Distinct().Count())
            throw new InvalidOperationException("Duplicate student entries are not allowed in a bulk request.");

        var distinctStudentIds = dto.Entries.Select(e => e.StudentID).Distinct().ToList();
        foreach (var studentId in distinctStudentIds)
            await EnsureStudentInClassAsync(studentId, dto.ClassID);

        var existing = await _db.Attendances
            .Where(a => a.ClassID == dto.ClassID && a.AttendanceDate == dto.Date)
            .ToListAsync();

        var byStudent = existing.ToDictionary(a => a.StudentID);
        var now = DateTime.Now;
        var affected = 0;

        foreach (var entry in dto.Entries)
        {
            if (byStudent.TryGetValue(entry.StudentID, out var row))
            {
                row.Status = entry.Status;
                row.Remarks = entry.Remarks;
                row.RecordedBy = recordedByUserId;
                row.UpdatedAt = now;
            }
            else
            {
                var entity = new Attendance
                {
                    AttendanceId = Guid.NewGuid(),
                    StudentID = entry.StudentID,
                    ClassID = dto.ClassID,
                    AttendanceDate = dto.Date,
                    Status = entry.Status,
                    Remarks = entry.Remarks,
                    RecordedBy = recordedByUserId,
                    TenantId = tenantId,
                    CreatedAt = now
                };
                _db.Attendances.Add(entity);
                byStudent[entry.StudentID] = entity;
            }

            affected++;
        }

        await _db.SaveChangesAsync();
        return affected;
    }
}
