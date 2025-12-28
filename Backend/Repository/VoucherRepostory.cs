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
    private readonly TenantDbContext _context;
    private readonly IMapper _mapper;
    private readonly mangeFilesService _mangeFilesService;

    public VoucherRepository(TenantDbContext context, IMapper mapper, mangeFilesService mangeFilesService)
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
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Year)
            .Include(v => v.AccountStudentGuardians)
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(s => s.Year)
            .Include(v => v.AccountStudentGuardians)
                .ThenInclude(v => v.Accounts)
            .Include(v => v.Attachments)
            .Where(v => v.AccountStudentGuardians != null && 
                       v.AccountStudentGuardians.Student != null &&
                       v.AccountStudentGuardians.Student.Division != null &&
                       v.AccountStudentGuardians.Student.Division.Class != null &&
                       ((v.AccountStudentGuardians.Student.Division.Class.Year != null && v.AccountStudentGuardians.Student.Division.Class.Year.Active == true) || 
                        (v.AccountStudentGuardians.Student.Division.Class.Stage != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year.Active == true)))
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
            AccountName = v.AccountStudentGuardians?.Accounts?.AccountName ?? string.Empty,
            AccountAttachments = v.Attachments?.Count ?? 0,
            AccountStudentGuardianID = v.AccountStudentGuardianID,
            StudentID = v.AccountStudentGuardians?.StudentID ?? 0,
        }).ToList();
        return vouchersDTO;
    }

    // إرجاع السندات مع التصفح
    public async Task<List<VouchersReturnDTO>> GetVouchersPaginatedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            throw new ArgumentException("Page number and page size must be greater than zero.");

        var vouchers = await _context.Vouchers
            .Include(v => v.AccountStudentGuardians)
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Year)
            .Include(v => v.AccountStudentGuardians)
                .ThenInclude(asg => asg.Student)
                    .ThenInclude(s => s.Division)
                        .ThenInclude(d => d.Class)
                            .ThenInclude(c => c.Stage)
                                .ThenInclude(s => s.Year)
            .Include(v => v.AccountStudentGuardians)
                .ThenInclude(v => v.Accounts)
            .Include(v => v.Attachments)
            .Where(v => v.AccountStudentGuardians != null && 
                       v.AccountStudentGuardians.Student != null &&
                       v.AccountStudentGuardians.Student.Division != null &&
                       v.AccountStudentGuardians.Student.Division.Class != null &&
                       ((v.AccountStudentGuardians.Student.Division.Class.Year != null && v.AccountStudentGuardians.Student.Division.Class.Year.Active == true) || 
                        (v.AccountStudentGuardians.Student.Division.Class.Stage != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year.Active == true)))
            .OrderByDescending(v => v.HireDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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
            AccountName = v.AccountStudentGuardians?.Accounts?.AccountName ?? string.Empty,
            AccountAttachments = v.Attachments?.Count ?? 0,
            AccountStudentGuardianID = v.AccountStudentGuardianID,
            StudentID = v.AccountStudentGuardians?.StudentID ?? 0,
        }).ToList();
        return vouchersDTO;
    }

    // إرجاع العدد الإجمالي للسندات
    public async Task<int> GetTotalVouchersCountAsync()
    {
        return await _context.Vouchers
            .Where(v => v.AccountStudentGuardians != null && 
                       v.AccountStudentGuardians.Student != null &&
                       v.AccountStudentGuardians.Student.Division != null &&
                       v.AccountStudentGuardians.Student.Division.Class != null &&
                       ((v.AccountStudentGuardians.Student.Division.Class.Year != null && v.AccountStudentGuardians.Student.Division.Class.Year.Active == true) || 
                        (v.AccountStudentGuardians.Student.Division.Class.Stage != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year != null && v.AccountStudentGuardians.Student.Division.Class.Stage.Year.Active == true)))
            .CountAsync();
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

        await _mangeFilesService.RemoveAttachmentsAsync("Attachments", voucher.VoucherID);

        _context.Vouchers.Remove(voucher);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VouchersGuardianDTO>> GetAllVouchersGuardian()
    {
        var students = await _context.Students
            .Include(s => s.AccountStudentGuardians)
                .ThenInclude(a => a.Vouchers)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Year)
            .Include(s => s.Division)
                .ThenInclude(d => d.Class)
                    .ThenInclude(c => c.Stage)
                        .ThenInclude(s => s.Year)
            .Include(s => s.Attachments)
            .Where(s => s.Division != null && 
                       s.Division.Class != null && 
                       ((s.Division.Class.Year != null && s.Division.Class.Year.Active == true) || 
                        (s.Division.Class.Stage != null && s.Division.Class.Stage.Year != null && s.Division.Class.Stage.Year.Active == true)))
            .ToListAsync();

        return students.Select(vg => new VouchersGuardianDTO()
        {
            StudentName = vg.FullName != null 
                ? $"{vg.FullName.FirstName} {vg.FullName.MiddleName} {vg.FullName.LastName}".Trim()
                : string.Empty,
            GuardianID = vg.GuardianID,
            ClassName = vg.Division?.Class?.ClassName ?? string.Empty,
            RequiredFee = vg.AccountStudentGuardians != null && vg.AccountStudentGuardians.Any()
                ? vg.AccountStudentGuardians.Select(a => a.Amount).ToList()
                : new List<decimal>(),
            receiptionFee = vg.AccountStudentGuardians != null && vg.AccountStudentGuardians.Any()
                ? vg.AccountStudentGuardians
                    .Where(a => a.Vouchers != null && a.Vouchers.Any())
                    .SelectMany(a => a.Vouchers)
                    .Sum(v => v.Receipt)
                : 0,
            ImageURL = vg.Attachments != null && vg.Attachments.Any()
                ? vg.Attachments.Select(a => a.AttachmentURL ?? string.Empty).Where(url => !string.IsNullOrEmpty(url)).ToList()
                : new List<string>(),
        }).ToList();
    }
}
