using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.Data;
using Backend.DTOS.School.Vouchers;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class VoucherRepository:IVoucherRepository
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;

    public VoucherRepository(DatabaseContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // إرجاع جميع السندات
    public async Task<List<VouchersDTO>> GetAllAsync()
    {
        var vouchers= await _context.Vouchers
            .Include(v => v.AccountStudentGuardians)
            .Include(v => v.Attachments)
            .ToListAsync();
         if (vouchers == null)
            return new List<VouchersDTO>();
        var vouchersDTO = _mapper.Map<List<VouchersDTO>>(vouchers);
        return vouchersDTO;
    }

    // إرجاع سند حسب المعرف
    public async Task<VouchersDTO?> GetByIdAsync(int id)
    {
        var voucher= await _context.Vouchers
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
        voucherDTO.VoucherID= voucher.VoucherID;
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
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher == null)
            return false;

        _context.Vouchers.Remove(voucher);
        await _context.SaveChangesAsync();
        return true;
    }
}
