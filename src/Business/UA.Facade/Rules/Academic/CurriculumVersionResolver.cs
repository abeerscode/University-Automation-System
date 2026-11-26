using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Academic;
public record CurriculumCandidate(int CurriculumId, int ProgramId, short EffectiveYear, int VersionNo, string Status);
public interface ICurriculumVersionResolver
{
    int Resolve(int programId, int admissionYear, IEnumerable<CurriculumCandidate> versions);
}
public sealed class CurriculumVersionResolver : ICurriculumVersionResolver
{
    public int Resolve(int programId, int admissionYear, IEnumerable<CurriculumCandidate> versions)
    {
        var matching = versions.Where(x => x.ProgramId == programId && x.EffectiveYear <= admissionYear &&
            (x.Status == "Active" || x.Status == "Retired"))
            .OrderByDescending(x => x.EffectiveYear).ThenByDescending(x => x.VersionNo).FirstOrDefault();
        return matching?.CurriculumId ?? throw new BusinessRuleException("No approved curriculum applies to this admission year.");
    }
}
