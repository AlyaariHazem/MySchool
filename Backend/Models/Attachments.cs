using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models;

public class Attachments
{
    public int AttachmentID { get; set; }
    public string AttachmentURL { get; set;}

    public int? StudentID { get; set; }
    public Student Student { get; set; }
    
    public int? VoucherID { get; set; }
    public Vouchers Vouchers { get; set; }
}