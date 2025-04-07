using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Vouchers;
using Backend.Models;

namespace Backend.Interfaces;

public interface IVoucherRepository
{
    Task<List<VouchersDTO>> GetAllAsync();
    Task<VouchersDTO?> GetByIdAsync(int id);
    Task<VouchersDTO> AddAsync(VouchersDTO voucher);
    Task<bool> UpdateAsync(VouchersDTO voucher);
    Task<bool> DeleteAsync(int id);
}