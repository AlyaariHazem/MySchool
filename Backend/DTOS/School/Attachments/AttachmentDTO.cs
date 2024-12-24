using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOS.School.Attachments;

public class AttachmentDTO
{
    public int AttachmentID { get; set; }
    public string AttachmentURL { get; set; }
    public string Note { get; set; }
    public int? VoucherID { get; set; }
}
