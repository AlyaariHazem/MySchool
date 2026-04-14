namespace Backend.Common;

/// <summary>Permission names: {Page}.{Action}. Used in JWT <c>permission</c> claims and <see cref="Authorization.HasPermissionAttribute"/>.</summary>
public static class PagePermissionNames
{
    public const string ActionView = "View";
    public const string ActionCreate = "Create";
    public const string ActionUpdate = "Update";
    public const string ActionDelete = "Delete";

    // Pages
    public const string PageDashboard = "Dashboard";
    public const string PageEmployees = "Employees";
    public const string PageTeachers = "Teachers";
    public const string PageStudents = "Students";
    public const string PageEvaluations = "Evaluations";
    public const string PageReports = "Reports";
    public const string PagePlans = "Plans";
    public const string PageActivities = "Activities";
    public const string PageComplaints = "Complaints";
    public const string PageMeetings = "Meetings";
    public const string PageRequests = "Requests";
    public const string PageSettings = "Settings";

    public static string P(string page, string action) => $"{page}.{action}";

    /// <summary>Every defined permission string (for seeding and admin “grant all”).</summary>
    public static readonly string[] All =
    {
        P(PageDashboard, ActionView),
        P(PageEmployees, ActionView), P(PageEmployees, ActionCreate), P(PageEmployees, ActionUpdate), P(PageEmployees, ActionDelete),
        P(PageTeachers, ActionView), P(PageTeachers, ActionCreate), P(PageTeachers, ActionUpdate), P(PageTeachers, ActionDelete),
        P(PageStudents, ActionView), P(PageStudents, ActionCreate), P(PageStudents, ActionUpdate), P(PageStudents, ActionDelete),
        P(PageEvaluations, ActionView), P(PageEvaluations, ActionCreate), P(PageEvaluations, ActionUpdate), P(PageEvaluations, ActionDelete),
        P(PageReports, ActionView), P(PageReports, ActionCreate), P(PageReports, ActionUpdate), P(PageReports, ActionDelete),
        P(PagePlans, ActionView), P(PagePlans, ActionCreate), P(PagePlans, ActionUpdate), P(PagePlans, ActionDelete),
        P(PageActivities, ActionView), P(PageActivities, ActionCreate), P(PageActivities, ActionUpdate), P(PageActivities, ActionDelete),
        P(PageComplaints, ActionView), P(PageComplaints, ActionCreate), P(PageComplaints, ActionUpdate), P(PageComplaints, ActionDelete),
        P(PageMeetings, ActionView), P(PageMeetings, ActionCreate), P(PageMeetings, ActionUpdate), P(PageMeetings, ActionDelete),
        P(PageRequests, ActionView), P(PageRequests, ActionCreate), P(PageRequests, ActionUpdate), P(PageRequests, ActionDelete),
        P(PageSettings, ActionView), P(PageSettings, ActionCreate), P(PageSettings, ActionUpdate), P(PageSettings, ActionDelete),
    };

    public const string ClaimType = "permission";
    public const string SchoolRoleClaimType = "SchoolRole";
}
