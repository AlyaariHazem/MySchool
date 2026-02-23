using System.Collections.Generic;
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
        Task<List<WeeklyScheduleDTO>> GetAllAsync();
        Task BulkUpdateAsync(List<AddWeeklyScheduleDTO> schedules);
    }
}
