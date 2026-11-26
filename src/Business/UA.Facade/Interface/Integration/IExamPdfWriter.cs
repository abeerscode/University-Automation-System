namespace UA.Facade.Interface.Integration;
public interface IExamPdfWriter { byte[] Write(string title,string[] headers,string[][] rows); }
