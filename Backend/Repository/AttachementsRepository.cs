using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOS.School.Attachments;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;


public class AttachmentsRepository : IAttachmentRepository
{
    private readonly DatabaseContext _context;

    public AttachmentsRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<AttachmentDTO> AddAsync(AttachmentDTO dto)
    {
        var entity = new Attachments
        {
            StudentID=dto.StudentID,
            AttachmentURL = dto.AttachmentURL,
            VoucherID = dto.VoucherID
        };

        _context.Attachments.Add(entity);
        await _context.SaveChangesAsync();

        dto.AttachmentID = entity.AttachmentID;
        return dto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Attachments.FindAsync(id);
        if (entity == null) return false;

        _context.Attachments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AttachmentDTO>> GetAllAsync()
    {
        return await _context.Attachments
            .Select(a => new AttachmentDTO
            {
                AttachmentID = a.AttachmentID,
                AttachmentURL = a.AttachmentURL,
                VoucherID = a.VoucherID
            }).ToListAsync();
    }

    public async Task<AttachmentDTO?> GetByIdAsync(int id)
    {
        var entity = await _context.Attachments.FindAsync(id);
        if (entity == null) return null;

        return new AttachmentDTO
        {
            AttachmentID = entity.AttachmentID,
            AttachmentURL = entity.AttachmentURL,
            VoucherID = entity.VoucherID
        };
    }

    public async Task<bool> UpdateAsync(AttachmentDTO dto)
    {
        var entity = await _context.Attachments.FindAsync(dto.AttachmentID);
        if (entity == null) return false;

        entity.AttachmentURL = dto.AttachmentURL;
        entity.VoucherID = dto.VoucherID;

        _context.Attachments.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
