using UA.DataAccess.Interface.Common;
using UA.Entities;
namespace UA.Facade.Rules.Student;
public static class RejoinCreditRecognition
{
 public static async Task<HashSet<int>> PassedAsync(string studentId,IRepository<Enrollment> enrollments,IRepository<Offering> offerings,IRepository<Result> results,IRepository<CourseEquivalence> equivalents,int targetTermId,CancellationToken ct)
 {
  var en=await enrollments.ListAsync(x=>x.StudentId==studentId,ct);var ids=en.Select(x=>x.EnrollmentId).ToArray();var os=en.Select(x=>x.OfferingId).ToArray();var offers=await offerings.ListAsync(x=>os.Contains(x.OfferingId)&&x.TermId!=targetTermId,ct);var rs=await results.ListAsync(x=>ids.Contains(x.EnrollmentId)&&x.PublishedAt!=null,ct);
  var latest=(from result in rs.GroupBy(x=>x.EnrollmentId).Select(g=>g.OrderByDescending(x=>x.VersionNo).First()) join n in en on result.EnrollmentId equals n.EnrollmentId join o in offers on n.OfferingId equals o.OfferingId select new {result,n,o}).GroupBy(x=>x.o.CourseId).Select(g=>g.OrderByDescending(x=>x.n.AttemptNo).ThenByDescending(x=>x.result.PublishedAt).First()).ToArray();
  var passed=latest.Where(x=>LifecycleRules.Passed(x.result.ResultStatus,x.result.LetterGrade,x.result.GradePoint)).Select(x=>x.o.CourseId).ToHashSet();
  var direct=passed.ToHashSet();foreach(var x in await equivalents.ListAsync(x=>x.StudentId==studentId,ct))if(direct.Contains(x.OldCourseId))passed.Add(x.NewCourseId);return passed;
 }
}
