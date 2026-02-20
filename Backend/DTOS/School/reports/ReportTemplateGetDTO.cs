using System;

namespace Backend.DTOS.School.reports
{
    public class ReportTemplateGetDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? SchoolId { get; set; }
        public string TemplateHtml { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
