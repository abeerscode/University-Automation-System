using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Academic;
public interface ISectionAllocator
{
    IReadOnlyList<(string Name, short Capacity)> Plan(int seats, int sectionCapacity);
    int IndexForMeritRoll(int roll, int sectionCapacity);
    int ChooseRejoinSection(IReadOnlyList<(int Count, int Capacity)> sections, int overflow);
}
public sealed class SectionAllocator : ISectionAllocator
{
    public IReadOnlyList<(string Name, short Capacity)> Plan(int seats, int sectionCapacity)
    {
        if (seats is < 1 or > 10000 || sectionCapacity is < 1 or > 1000)
            throw new BusinessRuleException("Seats and section capacity must be positive and within supported limits.");
        int count = (seats + sectionCapacity - 1) / sectionCapacity;
        if (count > 100) throw new BusinessRuleException("Too many sections.");
        return Enumerable.Range(0, count).Select(i => (Name: i < 26 ? ((char)('A' + i)).ToString() : $"S{i+1}", Capacity: (short)sectionCapacity)).ToArray();
    }
    public int IndexForMeritRoll(int roll, int sectionCapacity)
    {
        if (roll < 1 || sectionCapacity < 1) throw new BusinessRuleException("Invalid merit roll or section capacity.");
        return (roll - 1) / sectionCapacity;
    }
    public int ChooseRejoinSection(IReadOnlyList<(int Count, int Capacity)> sections, int overflow)
    {
        if (overflow < 0) throw new BusinessRuleException("Overflow cannot be negative.");
        var eligible = sections.Select((s, i) => (s.Count, s.Capacity, Index: i))
            .Where(s => s.Count < s.Capacity + overflow).OrderBy(s => s.Count).ThenBy(s => s.Index).ToList();
        if (eligible.Count == 0) throw new BusinessRuleException("No section has room; open a new section.");
        return eligible[0].Index;
    }
}
