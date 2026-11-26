namespace UA.Facade.Interface.Integration;
public interface IAcademicWorkbookWriter
{
    byte[] Write(IReadOnlyList<string> headers, IEnumerable<string[]> rows);
}
