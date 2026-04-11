using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Common;
using Backend.DTOS.School.reports;


namespace Backend.Interfaces;

public interface IReportRepository
{
    Task<Result<List<MonthlyResult>>> MonthlyReportsAsync(MonthlyReportQueryDTO query, int? guardianId = null);
    Task<Result<ReportTemplateGetDTO>> GetTemplateByCodeAsync(string code, int? schoolId);
    Task<Result<ReportTemplateGetDTO>> SaveTemplateAsync(ReportTemplateSaveDTO dto, int? schoolId);
}
