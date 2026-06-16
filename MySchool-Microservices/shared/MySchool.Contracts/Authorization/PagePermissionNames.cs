namespace MySchool.Contracts.Authorization;

/// <summary>Permission names: {Page}.{Action}. Used in JWT permission claims.</summary>
public static class PagePermissionNames
{
    public const string ActionView = "View";
    public const string ActionCreate = "Create";
    public const string ActionUpdate = "Update";
    public const string ActionDelete = "Delete";

    public const string PageDashboard = "Dashboard";
    public const string PageSettings = "Settings";
    public const string PageEmployees = "Employees";
    public const string PageRecruitment = "Recruitment";
    public const string PageTeachers = "Teachers";
    public const string PageStudents = "Students";
    public const string PageGuardians = "Guardians";
    public const string PageAccounts = "Accounts";
    public const string PageGrades = "Grades";
    public const string PageReports = "Reports";
    public const string PageReportsFinancial = "ReportsFinancial";
    public const string PageReportsTerm = "ReportsTerm";
    public const string PageReportsMonthly = "ReportsMonthly";
    public const string PageReportsRegistration = "ReportsRegistration";
    public const string PageReportsAllotment = "ReportsAllotment";
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
