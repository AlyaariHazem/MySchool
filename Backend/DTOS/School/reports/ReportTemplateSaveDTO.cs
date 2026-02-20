using System.ComponentModel.DataAnnotations;

namespace Backend.DTOS.School.reports
{
    public class ReportTemplateSaveDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        public int? SchoolId { get; set; }

        [Required]
        public string TemplateHtml { get; set; }
    }
}
