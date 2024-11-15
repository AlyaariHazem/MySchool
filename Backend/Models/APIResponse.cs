using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Models;

public class APIResponse
{
    public HttpStatusCode statusCode { get; set; }
    public bool IsSuccess { get; set; }=true;
    public List<string> ErrorMasseges { get; set; }

    public object Result { get; set; }
} 
