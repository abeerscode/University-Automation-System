using System.IO.Compression;
using System.Text;
using System.Xml;
using UA.Facade.Interface.Integration;
namespace UA.Integration.Reporting;
// Minimal Open XML workbook. Inline strings preserve leading zeroes and cannot execute formulas.
public sealed class AcademicWorkbookWriter : IAcademicWorkbookWriter
{
    private const string Ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    public byte[] Write(IReadOnlyList<string> headers, IEnumerable<string[]> rows)
    {
        using var output=new MemoryStream();
        using(var archive=new ZipArchive(output,ZipArchiveMode.Create,true))
        {
            void Part(string path,string content){using var stream=archive.CreateEntry(path).Open();using var w=new StreamWriter(stream,new UTF8Encoding(false));w.Write(content);}
            Part("[Content_Types].xml","""<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>""");
            Part("_rels/.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            Part("xl/workbook.xml","""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Academic report" sheetId="1" r:id="rId1"/></sheets></workbook>""");
            Part("xl/_rels/workbook.xml.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>""");
            using var sheet=archive.CreateEntry("xl/worksheets/sheet1.xml").Open();
            using var xml=XmlWriter.Create(sheet,new XmlWriterSettings{Encoding=new UTF8Encoding(false)});
            xml.WriteStartDocument();xml.WriteStartElement("worksheet",Ns);xml.WriteStartElement("sheetData",Ns);
            int index=0;
            void Row(IEnumerable<string> values)
            {
                xml.WriteStartElement("row",Ns);xml.WriteAttributeString("r",(++index).ToString(System.Globalization.CultureInfo.InvariantCulture));
                foreach(var value in values){xml.WriteStartElement("c",Ns);xml.WriteAttributeString("t","inlineStr");xml.WriteStartElement("is",Ns);xml.WriteStartElement("t",Ns);xml.WriteAttributeString("xml","space","http://www.w3.org/XML/1998/namespace","preserve");xml.WriteString(value??"");xml.WriteEndElement();xml.WriteEndElement();xml.WriteEndElement();}
                xml.WriteEndElement();
            }
            Row(headers);foreach(var row in rows)Row(row);
            xml.WriteEndElement();xml.WriteEndElement();xml.WriteEndDocument();
        }
        return output.ToArray();
    }
}
