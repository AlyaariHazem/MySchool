using System;
using System.Collections.Generic;

namespace Backend.DTOS.School.Accounts
{
    public class AccountReportDTO
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal? OpenBalance { get; set; }
        public bool TypeOpenBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }
        public List<AccountTransactionDTO> Transactions { get; set; } = new List<AccountTransactionDTO>();
        public List<AccountSavingsDTO> Savings { get; set; } = new List<AccountSavingsDTO>();
        public List<StudentInfoDTO> Students { get; set; } = new List<StudentInfoDTO>();
        public SchoolInfoDTO SchoolInfo { get; set; } = new SchoolInfoDTO();
    }

    public class SchoolInfoDTO
    {
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string SchoolPhone { get; set; } = string.Empty;
        public string SchoolLogo { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
    }

    public class AccountTransactionDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Debit" or "Credit"
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int? StudentID { get; set; } // Student ID for grouping transactions
    }

    public class AccountSavingsDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Type { get; set; } // true for savings, false for withdrawal
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class StudentInfoDTO
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? DivisionName { get; set; }
        public string? ClassName { get; set; }
        public string? StageName { get; set; }
    }
}
