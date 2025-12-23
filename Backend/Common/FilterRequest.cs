using System;
using System.Collections.Generic;

namespace Backend.Common;

public class FilterRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public Dictionary<string, FilterValue> Filters { get; set; } = new();
}

public class FilterValue
{
    public string? Value { get; set; }
    
    // Helper properties for type conversion
    public int? IntValue 
    { 
        get 
        { 
            if (int.TryParse(Value, out var intVal))
                return intVal;
            return null;
        } 
    }
    
    public DateTime? DateValue 
    { 
        get 
        { 
            if (DateTime.TryParse(Value, out var dateVal))
                return dateVal;
            return null;
        } 
    }
    
    public bool? BoolValue 
    { 
        get 
        { 
            if (bool.TryParse(Value, out var boolVal))
                return boolVal;
            return null;
        } 
    }
}

