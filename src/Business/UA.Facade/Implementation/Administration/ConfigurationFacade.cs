using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Administration;
using UA.Facade.Interface.Administration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
namespace UA.Facade.Implementation.Administration;
public sealed class ConfigurationFacade(IConfigParameterRepository configs, IRepository<AcademicTerm> terms,
    IUnitOfWork work, IUserAccessFacade access, ICurrentUser current) : IConfigurationFacade
{
    private static ConfigDto Map(ConfigParameter x) => new(x.ParameterId, x.DepartmentId, x.ParamKey, x.ParamValue, x.VersionNo, x.EffectiveTermId);
    public async Task<IReadOnlyList<ConfigDto>> HistoryAsync(string? key = null, CancellationToken ct = default)
    {
        await access.RequireAsync("config.manage", ct: ct);
        var list = string.IsNullOrWhiteSpace(key) ? await configs.ListAsync(ct: ct)
            : await configs.ListAsync(x => x.ParamKey == key, ct);
        // Config management is restricted to the departments granted to the user.
        var result = new List<ConfigDto>();
        foreach (var x in list.OrderByDescending(c => c.VersionNo).Take(500))
            if (await access.AllowedAsync(current.UserId!.Value, "config.manage", x.DepartmentId, ct)) result.Add(Map(x));
        return result;
    }
    public async Task<string?> ResolveAsync(string key, int? departmentId, int termId, CancellationToken ct = default)
    {
        // This read is intentionally used by future modules; they must authorize their caller separately.
        var wanted = await terms.FirstAsync(t => t.TermId == termId, ct) ?? throw new NotFoundException("Term not found.");
        var available = await configs.ListAsync(x => x.ParamKey == key && (x.DepartmentId == departmentId || x.DepartmentId == null), ct);
        var termIds = available.Select(c => c.EffectiveTermId).Distinct().ToArray();
        var termRows = await terms.ListAsync(t => termIds.Contains(t.TermId), ct);
        var matches = (from c in available join t in termRows on c.EffectiveTermId equals t.TermId
            where t.Year < wanted.Year || (t.Year == wanted.Year && t.TermNo <= wanted.TermNo)
            orderby (c.DepartmentId == departmentId && departmentId != null) descending, t.Year descending, t.TermNo descending, c.VersionNo descending
            select c).ToList();
        return matches.FirstOrDefault()?.ParamValue;
    }
    public async Task<ConfigDto> SetAsync(string key, string value, int? departmentId, int termId, CancellationToken ct = default)
    {
        if (departmentId is null) await access.RequireGlobalAsync("config.manage", ct);
        else await access.RequireAsync("config.manage", departmentId, ct);
        if (key.Length is < 1 or > 60 || value.Length > 255) throw new BusinessRuleException("Configuration key or value exceeds field size.");
        if (key == UA.Framework.Constants.ConfigKeys.AttendanceMinimum &&
            (!decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var percentage)
              || percentage < 75 || percentage > 100))
            throw new BusinessRuleException("Attendance threshold must be at least 75% and no more than 100%.");
        if (key == UA.Framework.Constants.ConfigKeys.NormalCredits &&
            (!int.TryParse(value, out var credits) || credits is < 12 or > 18))
            throw new BusinessRuleException("Normal credit load must be between 12 and 18.");
        var term = await terms.FirstAsync(x => x.TermId == termId, ct) ?? throw new NotFoundException("Term not found.");
        // Existing and past terms must not change effective rule versions.
        var latest = (await terms.ListAsync(ct: ct)).OrderByDescending(t => t.Year).ThenByDescending(t => t.TermNo).First();
        if (term.Year < latest.Year || (term.Year == latest.Year && term.TermNo < latest.TermNo))
            throw new BusinessRuleException("Configuration changes must apply to the current or a future academic term.");
        if (current.UserId is not int actor) throw new ForbiddenException();
        ConfigParameter? x = null;
        await work.ExecuteTransactionAsync(async token =>
        {
            var old = await configs.ListAsync(c => c.DepartmentId == departmentId && c.ParamKey == key, token);
            x = new ConfigParameter { DepartmentId = departmentId, ParamKey = key,
                ParamValue = value, EffectiveTermId = termId, ChangedBy = actor,
                VersionNo = old.Select(c => c.VersionNo).DefaultIfEmpty(0).Max() + 1 };
            await configs.AddAsync(x, token);
        }, ct);
        return Map(x!);
    }
}
