namespace UA.Facade.Rules.Examination;

public sealed record ExamSlot(DateTime Date, TimeSpan Start, TimeSpan End)
{
    public bool Overlaps(ExamSlot other) => Date.Date == other.Date.Date && Start < other.End && other.Start < End;
}
public sealed record ExamHall(int Id, int Capacity);
public sealed record ReservedExamSeat(string StudentId, int RoomId, string SeatNo);
public sealed record ExamDemand(int Key, ExamSlot Slot, bool Regular, int[] BatchIds, int[] SectionIds,
    string[] StudentIds, ReservedExamSeat[] FixedSeats, string[]? RetakeStudentIds = null, bool Special = false);

/// <summary>Conservative, deterministic room-exclusive timetable feasibility rule. No database writes.</summary>
public static class ExamScheduleRules
{
    public static bool TryPlan(IReadOnlyList<ExamDemand> exams, IReadOnlyList<ExamHall> halls,
        out Dictionary<int, HashSet<int>> plan, out string reason)
    {
        var assigned = new Dictionary<int, HashSet<int>>();
        plan = assigned;
        reason = "";
        var capacities = halls.ToDictionary(x => x.Id, x => x.Capacity);
        var demand = exams.ToDictionary(x => x.Key, x => x.StudentIds.Concat(x.RetakeStudentIds ?? []).Concat(x.FixedSeats.Select(s => s.StudentId)).Distinct().Count());
        foreach (var exam in exams)
        {
            assigned.Add(exam.Key, exam.FixedSeats.Select(s => s.RoomId).ToHashSet());
            if (exam.FixedSeats.Select(s => s.StudentId).Distinct().Count() != exam.FixedSeats.Length ||
                exam.FixedSeats.Select(s => (s.RoomId, s.SeatNo)).Distinct().Count() != exam.FixedSeats.Length ||
                exam.FixedSeats.Any(s => !capacities.TryGetValue(s.RoomId, out var capacity) ||
                    !int.TryParse(s.SeatNo, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var number) ||
                    number < 1 || number > capacity || number.ToString(System.Globalization.CultureInfo.InvariantCulture) != s.SeatNo))
            { reason = "Existing exam seats are invalid or duplicated. Correct the seat plan first."; return false; }
        }
        for (int i = 0; i < exams.Count; i++)
        for (int j = i + 1; j < exams.Count; j++)
        {
            var a = exams[i]; var b = exams[j];
            if (!a.Slot.Overlaps(b.Slot)) continue;
            // BR-15: regular/retake clashes remain a manual Phase 8 workflow.
            if (a.Regular && b.Regular && (a.BatchIds.Intersect(b.BatchIds).Any() ||
                a.SectionIds.Intersect(b.SectionIds).Any() || a.StudentIds.Concat(a.FixedSeats.Select(s => s.StudentId)).Except(a.RetakeStudentIds ?? [])
                    .Intersect(b.StudentIds.Concat(b.FixedSeats.Select(s => s.StudentId)).Except(b.RetakeStudentIds ?? [])).Any()))
            { reason = "Exam slot clashes with a batch, section or regular examinee."; return false; }
            if(a.Special&&b.Special&&(a.RetakeStudentIds??[]).Intersect(b.RetakeStudentIds??[]).Any()) { reason="A student has two special retakes in the same slot."; return false; }
            if (assigned[a.Key].Overlaps(assigned[b.Key]))
            { reason = "Room already allocated to a concurrent sitting."; return false; }
        }
        // Reserve all previously allocated halls before selecting new halls. For remaining seats use
        // the smallest single hall that fits, otherwise the largest available hall, deterministically.
        foreach (var exam in exams.OrderByDescending(x => demand[x.Key]).ThenBy(x => x.Slot.Date)
                     .ThenBy(x => x.Slot.Start).ThenBy(x => x.Key))
        {
            var reserved = assigned[exam.Key];
            var unavailable = exams.Where(x => x.Key != exam.Key && x.Slot.Overlaps(exam.Slot))
                .SelectMany(x => assigned[x.Key]).ToHashSet();
            var remaining = demand[exam.Key] - reserved.Sum(r => capacities[r]);
            while (remaining > 0)
            {
                var available = halls.Where(h => !reserved.Contains(h.Id) && !unavailable.Contains(h.Id) && h.Capacity > 0).ToArray();
                var hall = available.Where(h => h.Capacity >= remaining).OrderBy(h => h.Capacity).ThenBy(h => h.Id).FirstOrDefault()
                    ?? available.OrderByDescending(h => h.Capacity).ThenBy(h => h.Id).FirstOrDefault();
                if (hall is null) { reason = "Insufficient exam hall capacity for concurrent sittings."; return false; }
                reserved.Add(hall.Id); remaining -= hall.Capacity;
            }
        }
        return true;
    }
}
