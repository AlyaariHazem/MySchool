using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOS.School.Terms;

namespace Backend.Interfaces;

public interface ITermRepository
{
    Task AddTermAsync(TermDTO term);
    Task<List<TermDTO>> GetAllTermsAsync();
    Task<TermDTO> GetTermByIdAsync(int id);
    Task UpdateTermAsync(TermDTO term);
    Task DeleteTermAsync(int id);
}
