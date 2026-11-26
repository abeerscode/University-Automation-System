using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Student;
public interface IStudentIdGenerator { string Generate(int admissionYear, string departmentIdCode, int roll); }
/// <summary>BR-04: YY + semester 1 + department 01–04 + merit roll 001–999.</summary>
public sealed class StudentIdGenerator : IStudentIdGenerator
{
    public string Generate(int admissionYear, string departmentIdCode, int roll)
    {
        if (admissionYear is < 2000 or > 2099 || departmentIdCode is not ("01" or "02" or "03" or "04") || roll is < 1 or > 999)
            throw new BusinessRuleException("Student ID requires year 2000–2099, valid department digits and roll 1–999.");
        return $"{admissionYear % 100:D2}1{departmentIdCode}{roll:D3}";
    }
}
