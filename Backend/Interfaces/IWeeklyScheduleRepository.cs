using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.DTOS.School.WeeklySchedule;
using Backend.Models;
using Backend.Repository;

namespace Backend.Repository.School.Interfaces
{
    public interface IWeeklyScheduleRepository : IgenericRepository<WeeklySchedule>
    {
        Task Add(AddWeeklyScheduleDTO obj);
        Task Update(UpdateWeeklyScheduleDTO obj);
        Task<List<WeeklyScheduleDTO>> GetByClassAndTermAsync(int classId, int termId, int? divisionId = null);
        Task<WeeklyScheduleGridDTO> GetScheduleGridAsync(int classId, int termId, int? divisionId = null);
        /// <summary>All weekly slots for a teacher in the active year and given term (merged across classes).</summary>
        Task<WeeklyScheduleGridDTO> GetScheduleGridForTeacherAsync(int teacherId, int termId);
        Task<List<WeeklyScheduleDTO>> GetAllAsync();
        Task BulkUpdateAsync(List<AddWeeklyScheduleDTO> schedules);

        /// <summary>
        /// Teacher occupied (day, period) slots for the year/term, excluding the timetable rows that belong to the
        /// same class (and optional division) being regenerated so teachers are not blocked by their own section.
        /// </summary>
        Task<HashSet<(int TeacherId, int Day, int Period)>> GetTeacherBusySlotsAsync(
            int yearId,
            int termId,
            int forClassId,
            int? forDivisionId,
            CancellationToken cancellationToken = default);
    }
}
