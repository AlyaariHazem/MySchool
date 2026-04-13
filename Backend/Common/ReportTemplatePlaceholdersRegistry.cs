using Backend.DTOS.School.reports;

namespace Backend.Common;

/// <summary>
/// Maps report template codes to supported #Placeholder# merge fields.
/// Keep in sync with frontend report components that replace these tokens.
/// </summary>
public static class ReportTemplatePlaceholdersRegistry
{
    public static IReadOnlyList<ReportTemplatePlaceholderDto> GetForCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Array.Empty<ReportTemplatePlaceholderDto>();

        return code.Trim().ToUpperInvariant() switch
        {
            "ACCOUNT_REPORT" => AccountReport,
            "STUDENT_MONTH_RESULT" => StudentMonthResult,
            "RECEIPT_VOUCHER" => ReceiptVoucher,
            "REGISTRATION_FORM" => RegistrationForm,
            _ => Generic
        };
    }

    /// <summary>Shared fields used by several student-facing reports.</summary>
    private static readonly ReportTemplatePlaceholderDto[] StudentMonthResult =
    [
        new() { Name = "FullName", Description = "Student full name" },
        new() { Name = "StudentId", Description = "Student ID" },
        new() { Name = "SID", Description = "Student number" },
        new() { Name = "SchoolYear", Description = "Academic year" },
        new() { Name = "ClassName", Description = "Class" },
        new() { Name = "DivisionName", Description = "Division" },
        new() { Name = "PhaseName", Description = "Phase" },
        new() { Name = "SchoolName", Description = "School name" },
        new() { Name = "SchoolAddress", Description = "School address" },
        new() { Name = "SchoolPhone", Description = "School phone" },
        new() { Name = "TermName", Description = "Term name" },
        new() { Name = "MonthName", Description = "Month name" },
        new() { Name = "Grade", Description = "Grade / score" },
        new() { Name = "SubjectName", Description = "Subject" },
        new() { Name = "CurrentDate", Description = "Current date" },
        new() { Name = "ReportDate", Description = "Report date" },
    ];

    private static readonly ReportTemplatePlaceholderDto[] ReceiptVoucher = StudentMonthResult;

    private static readonly ReportTemplatePlaceholderDto[] RegistrationForm =
    [
        ..StudentMonthResult,
        new() { Name = "Age", Description = "Age" },
        new() { Name = "Address", Description = "Address" },
        new() { Name = "Sex", Description = "Gender" },
        new() { Name = "Birthplace", Description = "Place of birth" },
        new() { Name = "Phone", Description = "Phone" },
    ];

    private static readonly ReportTemplatePlaceholderDto[] AccountReport =
    [
        new() { Name = "AccountNo", Description = "Account number" },
        new() { Name = "Guardian", Description = "Guardian / account name" },
        new() { Name = "CreatedDate", Description = "Account creation date" },
        new() { Name = "TotalDebit", Description = "Total debit" },
        new() { Name = "TotalCredit", Description = "Total credit" },
        new() { Name = "Balance", Description = "Balance" },
        new() { Name = "SchoolName", Description = "School name" },
        new() { Name = "SchoolAddress", Description = "School address" },
        new() { Name = "SchoolPhone", Description = "School phone" },
        new() { Name = "SchoolLogo", Description = "School logo URL" },
        new() { Name = "SchoolYear", Description = "Academic year" },
        new() { Name = "HeaderMessage", Description = "Header message (plain)" },
        new() { Name = "HeaderMessageBlock", Description = "Header message (HTML block)" },
        new() { Name = "StudentsInfo", Description = "Students information block" },
        new() { Name = "TransactionsTable", Description = "Transactions table rows" },
        new() { Name = "SavingsTable", Description = "Savings / deposits table rows" },
        new() { Name = "TotalSavings", Description = "Total savings" },
    ];

    private static readonly ReportTemplatePlaceholderDto[] Generic =
    [
        new() { Name = "SchoolName", Description = "School name" },
        new() { Name = "SchoolYear", Description = "Academic year" },
        new() { Name = "CurrentDate", Description = "Current date" },
    ];
}
