using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ReportTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        public int? SchoolId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string TemplateHtml { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property (optional, for future use)
        [JsonIgnore]
        public School? School { get; set; }
    }
}
