using System.Globalization;
using System.Text;
using UA.Facade.DTO.Finance;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Reporting;
// Minimal dependency-free PDF 1.4 writer; only ASCII receipt identifiers and amounts.
public sealed class ReceiptPdfWriter : IReceiptPdfWriter
{
    public byte[] Write(ReceiptDto r)
    {
        var lines = new List<string>{"UNIVERSITY AUTOMATION SYSTEM", "COUNTER PAYMENT RECEIPT", "Receipt: " + r.Number,"Student: " + r.StudentId,"Issued: " + r.IssuedAt.ToString("yyyy-MM-dd HH:mm",CultureInfo.InvariantCulture),"Total: BDT " + r.Amount.ToString("F2",CultureInfo.InvariantCulture),"", "Allocations:"}; lines.AddRange(r.Allocations);
        var pages=lines.Chunk(40).ToArray(); var objs=new List<string>{"<< /Type /Catalog /Pages 2 0 R >>", "", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"}; var kids=new List<string>();
        foreach(var page in pages) { int pageId=objs.Count+1, streamId=pageId+1; kids.Add($"{pageId} 0 R"); var content="BT /F1 12 Tf 48 790 Td 18 TL\n"+string.Join("\n",page.Select(x=>"("+x.Replace("\\","\\\\").Replace("(","\\(").Replace(")","\\)")+") Tj T*"))+"\nET\n"; objs.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {streamId} 0 R >>"); objs.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream"); }
        objs[1]=$"<< /Type /Pages /Count {pages.Length} /Kids [{string.Join(" ",kids)}] >>";
        var text=new StringBuilder("%PDF-1.4\n"); var offsets=new List<int>{0}; foreach(var obj in objs) { offsets.Add(text.Length); text.Append($"{offsets.Count-1} 0 obj\n{obj}\nendobj\n"); } int xref=text.Length; text.Append($"xref\n0 {offsets.Count}\n0000000000 65535 f \n"); foreach(int offset in offsets.Skip(1)) text.Append(offset.ToString("D10",CultureInfo.InvariantCulture)+" 00000 n \n"); text.Append($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n"); return Encoding.ASCII.GetBytes(text.ToString());
    }
}
