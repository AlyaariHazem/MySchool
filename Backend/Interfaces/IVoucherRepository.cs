using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Vouchers;
using Backend.Models;

namespace Backend.Interfaces;

public interface IVoucherRepository
{
    Task<List<VouchersReturnDTO>> GetAllAsync();
    Task<List<VouchersReturnDTO>> GetVouchersPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalVouchersCountAsync();
    Task<VouchersDTO?> GetByIdAsync(int id);
    Task<List<VouchersGuardianDTO>> GetAllVouchersGuardian(int? guardianID = null);
    Task<VouchersDTO> AddAsync(VouchersDTO voucher);
    Task<bool> UpdateAsync(VouchersDTO voucher);
    Task<bool> DeleteAsync(int id);
}