using UA.Entities;
using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Examination;
public static class RetakeSchemeRules
{
 public static string Key(string type)=>type=="Theory"?"TheoryRetake":"R:"+type;
 public static bool IsRetake(string key)=>key=="TheoryRetake"||key.StartsWith("R:",StringComparison.Ordinal);
 public static MarkComponent[] Resolve(IEnumerable<MarkComponent> rows,DateTime date,string type)
 {
  var available=rows.Where(x=>x.CourseType==Key(type)&&x.EffectiveFrom.Date<=date.Date).ToArray();
  var latest=available.OrderByDescending(x=>x.EffectiveFrom).ThenByDescending(x=>x.VersionNo).FirstOrDefault();
  var parts=available.Where(x=>latest!=null&&x.VersionNo==latest.VersionNo&&x.EffectiveFrom==latest.EffectiveFrom).OrderBy(x=>x.ComponentId).ToArray();
  if(parts.Length==0||parts.Any(x=>x.IsAttendanceBased||x.MaxMarks<=0||x.MaxMarks>100)||parts.Sum(x=>x.MaxMarks)!=100|| (type=="Theory"&&parts.Length!=1))throw new BusinessRuleException("Configure an exam-only retake scheme totaling 100 marks for this department and course type first.");
  return parts;
 }
}
