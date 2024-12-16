using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.StudentClassFee;

public class StudentClassFeeDTO
{
    public int StudentClassFeesID { get; set; }
    public int StudentID { get; set; }
    public int FeeClassID  { get; set; }
    public decimal? AmountDiscount { get; set; }
    public string? NoteDiscount { get; set; }
    public List<string>?  Files { get; set; }
}
