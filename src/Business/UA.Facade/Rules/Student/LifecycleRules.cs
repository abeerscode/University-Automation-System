namespace UA.Facade.Rules.Student;
public static class LifecycleRules
{
    public static DateTime Limit(int admissionYear)=>new(admissionYear+6,12,31);
    public static int Semester(int admissionYear,int year,int term)=> (year-admissionYear)*2+term;
    public static bool CanRetake(int existingCount)=>existingCount<2;
    public static bool Passed(string status,string grade,decimal points)=>status=="Normal"&&grade!="F"&&points>=2;
}
