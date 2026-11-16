namespace UA.Framework.Constants;
public static class PermissionCodes
{
    // Appendix B of the development blueprint. The database holds the active grants.
    public static readonly string[] All = (
        "users.view users.manage roles.manage approvals.record approvals.correct audit.view config.manage templates.manage " +
        "imports.run backup.manage dashboards.view exports.sensitive " +
        "departments.manage curriculum.manage courses.manage calendar.manage batches.manage rooms.manage offerings.manage " +
        "registration.run retake.enroll retake.override routine.manage attendance.view reports.view " +
        "students.view students.edit status.change section.change drop.request drop.recommend drop.approve rejoin.place " +
        "equivalence.decide graduation.process limit.decide requests.process " +
        "faculty.view faculty.manage assignments.manage mentors.manage workload.view " +
        "exams.manage routine.view seatplan.manage eligibility.view hold.release.attendance hold.release.dues marks.enter " +
        "marks.verify marks.approve results.publish results.correct transcripts.issue makeup.manage " +
        "fees.manage dues.view payments.record payments.reverse ledger.view reconciliation.view finance.reports waivers.manage " +
        "circulars.manage applications.review merit.manage enrollment.confirm enrollment.replace admission.reports " +
        "pages.edit pages.approve notices.edit notices.approve departmentpage.edit " +
        "notifications.view jobs.view files.download").Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
