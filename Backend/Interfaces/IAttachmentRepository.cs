using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Attachments;

namespace Backend.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<List<AttachmentDTO>> GetAllAsync();
        Task<AttachmentDTO?> GetByIdAsync(int id);
        Task<AttachmentDTO> AddAsync(AttachmentDTO voucher);
        Task<bool> UpdateAsync(AttachmentDTO voucher);
        Task<bool> DeleteAsync(int id);
    }

}

