namespace Backend.Common;

/// <summary>Permission names: {Page}.{Action}. Used in JWT <c>permission</c> claims and <see cref="Authorization.HasPermissionAttribute"/>.</summary>
public static class PagePermissionNames
{
    public const string ActionView = "View";
    public const string ActionCreate = "Create";
    public const string ActionUpdate = "Update";
    public const string ActionDelete = "Delete";

    // Pages (school shell modules / sidebar)
    public const string PageDashboard = "Dashboard";
    public const string PageSettings = "Settings";
    public const string PageEmployees = "Employees";
    /// <summary>Recruitment / hiring (job postings, applications, interviews).</summary>
    public const string PageRecruitment = "Recruitment";
    public const string PageTeachers = "Teachers";
    public const string PageStudents = "Students";
    public const string PageGuardians = "Guardians";
    public const string PageAccounts = "Accounts";
    public const string PageGrades = "Grades";
    public const string PageReports = "Reports";
    /// <summary>Financial / account reports (route: <c>reports/account</c>).</summary>
    public const string PageReportsFinancial = "ReportsFinancial";
    /// <summary>Term grades report (<c>reports/term-result</c>).</summary>
    public const string PageReportsTerm = "ReportsTerm";
    /// <summary>Monthly grades report (<c>reports/grades-month</c>).</summary>
    public const string PageReportsMonthly = "ReportsMonthly";
    /// <summary>Registration form report (<c>reports/registration</c>).</summary>
    public const string PageReportsRegistration = "ReportsRegistration";
    /// <summary>Allotment / custom report layout (<c>reports/allotment</c>).</summary>
    public const string PageReportsAllotment = "ReportsAllotment";
    /// <summary>Floating AI assistant (<c>api/Ai/chat</c>).</summary>
    public const string PageAiChat = "AiChat";

    public const string PageCalendar = "Calendar";
    public const string PageSchedule = "Schedule";
    public const string PageExams = "Exams";
    public const string PageHomework = "Homework";
    public const string PageAttendance = "Attendance";
    public const string PageNotifications = "Notifications";
    public const string PageTests = "Tests";
    public const string PageHolidays = "Holidays";
    public const string PageEvents = "Events";
    public const string PageFees = "Fees";
    public const string PageCourses = "Courses";
    public const string PagePayroll = "Payroll";
    public const string PageBlogs = "Blogs";
    public const string PageManagement = "Management";

    public const string PageEvaluations = "Evaluations";
    public const string PagePlans = "Plans";
    public const string PageActivities = "Activities";
    public const string PageComplaints = "Complaints";
    public const string PageMeetings = "Meetings";
    public const string PageRequests = "Requests";

    public static string P(string page, string action) => $"{page}.{action}";

    private static string[] ActionsFor(string page) =>
    [
        P(page, ActionView),
        P(page, ActionCreate),
        P(page, ActionUpdate),
        P(page, ActionDelete),
    ];

    /// <summary>Every defined permission string (for seeding, policies, and admin “grant all”).</summary>
    public static readonly string[] All = BuildAll();

    private static string[] BuildAll()
    {
        var pages = new[]
        {
            PageDashboard,
            PageSettings,
            PageEmployees,
            PageRecruitment,
            PageTeachers,
            PageStudents,
            PageGuardians,
            PageAccounts,
            PageGrades,
            PageReports,
            PageReportsFinancial,
            PageReportsTerm,
            PageReportsMonthly,
            PageReportsRegistration,
            PageReportsAllotment,
            PageAiChat,
            PageCalendar,
            PageSchedule,
            PageExams,
            PageHomework,
            PageAttendance,
            PageNotifications,
            PageTests,
            PageHolidays,
            PageEvents,
            PageFees,
            PageCourses,
            PagePayroll,
            PageBlogs,
            PageManagement,
            PageEvaluations,
            PagePlans,
            PageActivities,
            PageComplaints,
            PageMeetings,
            PageRequests,
        };

        var list = new List<string> { P(PageDashboard, ActionView) };
        foreach (var page in pages)
        {
            if (page == PageDashboard)
                continue;
            list.AddRange(ActionsFor(page));
        }

        return list.ToArray();
    }

    public const string ClaimType = "permission";
    public const string SchoolRoleClaimType = "SchoolRole";
}
