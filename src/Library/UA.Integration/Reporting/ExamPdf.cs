using System.Text;
using System.Globalization;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Reporting;
public sealed class ExamPdf : IExamPdfWriter
{
    public byte[] Write(string title,string[] headers,string[][] rows)
    {
        // Standard PDF font supports ASCII identifiers; use browser print for full Unicode text.
        string Safe(string s)=>new string(s.Select(c=>c is >= ' ' and <= '~'?c:'?').ToArray()).Replace("\\","\\\\").Replace("(","\\(").Replace(")","\\)");
        var lines=new List<string>();foreach(var row in rows){for(int i=0;i<headers.Length;i++){var line=headers[i]+": "+row[i];foreach(var segment in line.Chunk(92))lines.Add(new string(segment));}lines.Add("");}if(lines.Count==0)lines.Add("No records");
        var objs=new List<string>{"<< /Type /Catalog /Pages 2 0 R >>","","<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"};var kids=new List<string>();foreach(var page in lines.Chunk(43)){int id=objs.Count+1;kids.Add($"{id} 0 R");var content="BT /F1 11 Tf 42 800 Td 16 TL\n"+string.Join("\n",new[]{"University Automation System",title,""}.Concat(page).Select(x=>"("+Safe(x)+") Tj T*"))+"\nET\n";objs.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {id+1} 0 R >>");objs.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream");}objs[1]=$"<< /Type /Pages /Count {kids.Count} /Kids [{string.Join(" ",kids)}] >>";var text=new StringBuilder("%PDF-1.4\n");var offsets=new List<int>{0};foreach(var obj in objs){offsets.Add(text.Length);text.Append($"{offsets.Count-1} 0 obj\n{obj}\nendobj\n");}int xref=text.Length;text.Append($"xref\n0 {offsets.Count}\n0000000000 65535 f \n");foreach(var offset in offsets.Skip(1))text.Append(offset.ToString("D10",CultureInfo.InvariantCulture)+" 00000 n \n");text.Append($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");return Encoding.ASCII.GetBytes(text.ToString());
    }
}
