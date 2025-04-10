using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Vouchers;
using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class VoucherRepository : IVoucherRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    private readonly mangeFilesService _mangeFilesService;

    public VoucherRepository(DatabaseContext context, IMapper mapper, mangeFilesService mangeFilesService)
    {
        _context = context;
        _mapper = mapper;
        _mangeFilesService = mangeFilesService;
    }

    // إرجاع جميع السندات
    public async Task<List<VouchersReturnDTO>> GetAllAsync()
    {
        var vouchers = await _context.Vouchers
            .Include(v => v.AccountStudentGuardians)
             .ThenInclude(v => v.Accounts)
            .Include(v => v.Attachments)
            .ToListAsync();
        if (vouchers == null)
            return new List<VouchersReturnDTO>();

        var vouchersDTO = vouchers.Select(v => new VouchersReturnDTO
        {
            VoucherID = v.VoucherID,
            Receipt = v.Receipt,
            Note = v.Note,
            PayBy = v.PayBy,
            HireDate = v.HireDate,
            AccountName = v.AccountStudentGuardians.Accounts.AccountName,
            AccountAttachments = v.Attachments.Count,
            StudentID = v.AccountStudentGuardians.StudentID,
        }).ToList();
        return vouchersDTO;
    }

    // إرجاع سند حسب المعرف
    public async Task<VouchersDTO?> GetByIdAsync(int id)
    {
        var voucher = await _context.Vouchers
            .Include(v => v.AccountStudentGuardians)
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.VoucherID == id);
        if (voucher == null)
            return null;
        var voucherDTO = _mapper.Map<VouchersDTO>(voucher);
        return voucherDTO;
    }

    // إنشاء سند جديد
    public async Task<VouchersDTO> AddAsync(VouchersDTO voucherDTO)
    {
        var voucher = _mapper.Map<Vouchers>(voucherDTO);
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();
        voucherDTO.VoucherID = voucher.VoucherID;
        return voucherDTO;
    }

    // تحديث سند
    public async Task<bool> UpdateAsync(VouchersDTO voucher)
    {
        var existing = await _context.Vouchers.FindAsync(voucher.VoucherID);
        if (existing == null)
            return false;

        existing.Receipt = voucher.Receipt;
        existing.Note = voucher.Note;
        existing.PayBy = voucher.PayBy!;
        existing.HireDate = voucher.HireDate;
        existing.AccountStudentGuardianID = voucher.AccountStudentGuardianID;
        // لا تنسَ التعامل مع المرفقات إذا لزم الأمر

        _context.Entry(existing).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    // حذف سند
    public async Task<bool> DeleteAsync(int id)
    {
        var voucher = await _context.Vouchers
        .Include(v => v.Attachments)
        .FirstOrDefaultAsync(v => v.VoucherID == id);

        if (voucher == null)
            return false;

        if (voucher.Attachments != null && voucher.Attachments.Count > 0)
            _context.Attachments.RemoveRange(voucher.Attachments);
        
        await _mangeFilesService.RemoveAttachmentsAsync(voucher.AccountStudentGuardianID, voucher.VoucherID);
        
        _context.Vouchers.Remove(voucher);
        await _context.SaveChangesAsync();
        return true;
    }
}
