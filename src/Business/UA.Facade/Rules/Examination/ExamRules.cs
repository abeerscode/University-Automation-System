namespace UA.Facade.Rules.Examination;
public static class ExamRules
{
    public static decimal Round(decimal value)=>Math.Round(value,2,MidpointRounding.AwayFromZero);
    public static decimal Total(IEnumerable<decimal> values)=>Round(values.Sum());
    public static decimal Gpa(IEnumerable<(decimal Credits,decimal Points)> values)
    {var rows=values.ToArray();var credits=rows.Sum(x=>x.Credits);return credits==0?0:Round(rows.Sum(x=>x.Credits*x.Points)/credits);}
    // Retake calculation foundation. Published attempt selection is separate from the Phase 8 retake workflow.
    public static decimal Cgpa(IEnumerable<(int CourseId,int Attempt,int Version,decimal Credits,decimal Points)> attempts)
        =>Gpa(attempts.GroupBy(x=>x.CourseId).Select(g=>g.OrderByDescending(x=>x.Attempt).ThenByDescending(x=>x.Version).First()).Select(x=>(x.Credits,x.Points)));
    public static bool InBand(decimal total,decimal min,decimal max)=>total>=min&&(total<max||(max==100m&&total==100m));
}
