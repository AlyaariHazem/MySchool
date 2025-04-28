using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.reports;


namespace Backend.Interfaces;

public interface IReportRepository
{
    Task<Result<List<MonthlyResult>>> GetAllReportsAsync();
}
