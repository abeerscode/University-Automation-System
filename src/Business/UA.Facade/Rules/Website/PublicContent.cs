using System.Text;
using System.Text.Encodings.Web;
using System.Xml;
using System.Xml.Linq;
using System.Text.Json;
namespace UA.Facade.Rules.Website;
public record SiteProfile(string Name,string Logo,string Address,string Email,string Telephone,string Facebook,string LinkedIn)
{
 public static SiteProfile Default=>new("University","","","","","","");
 public static SiteProfile Parse(string? value){try{return JsonSerializer.Deserialize<SiteProfile>(value??"")??Default;}catch(JsonException){return Default;}}
}
public static class PublicContent
{
 private static readonly HashSet<string> Tags=new(StringComparer.Ordinal){"p","h2","h3","h4","strong","em","ul","ol","li","blockquote","br","table","thead","tbody","tr","th","td","a","img"};
 private static string E(string value)=>HtmlEncoder.Default.Encode(value);
 public static bool LocalImage(string? s)=>s!=null&&System.Text.RegularExpressions.Regex.IsMatch(s,@"^/(images|cms-assets)/[a-zA-Z0-9/_-]+\.(png|jpg|jpeg|webp)$")&&!s.Contains("//");
 public static bool Https(string? s)=>Uri.TryCreate(s,UriKind.Absolute,out var u)&&u.Scheme=="https"&&string.IsNullOrEmpty(u.UserInfo);
 public static string Render(string? value)
 {
  value??="";if(!value.StartsWith("html:\n",StringComparison.Ordinal)&&!value.StartsWith("html:\r\n",StringComparison.Ordinal))return "<div style=\"white-space:pre-wrap\">"+E(value)+"</div>";
  try{using var sr=new StringReader("<root>"+value[(value.IndexOf('\n')+1)..]+"</root>");using var reader=XmlReader.Create(sr,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=35000});var root=XElement.Load(reader,LoadOptions.PreserveWhitespace);var b=new StringBuilder();foreach(var n in root.Nodes())Write(n,b,0);return b.ToString();}catch(XmlException){return "<p>"+E(value)+"</p>";}catch(InvalidOperationException){return "<p>"+E(value)+"</p>";}
 }
 private static void Write(XNode node,StringBuilder b,int depth)
 {
  if(depth>30)throw new InvalidOperationException();if(node is XText text){b.Append(E(text.Value));return;}if(node is not XElement x)return;
  var tag=x.Name.LocalName;if(x.Name.Namespace!=XNamespace.None||!Tags.Contains(tag))return;
  if(tag=="img"){var src=(string?)x.Attribute("src");if(LocalImage(src))b.Append("<img class=\"img-fluid\" loading=\"lazy\" src=\"").Append(E(src!)).Append("\" alt=\"").Append(E((string?)x.Attribute("alt")??"")).Append("\">");return;}
  if(tag=="br"){b.Append("<br>");return;}
  b.Append('<').Append(tag);if(tag=="a"){var href=(string?)x.Attribute("href");if(Https(href))b.Append(" href=\"").Append(E(href!)).Append("\" rel=\"noopener noreferrer\"");}if(tag=="table")b.Append(" class=\"table table-bordered\"");b.Append('>');foreach(var child in x.Nodes())Write(child,b,depth+1);b.Append("</").Append(tag).Append('>');
 }
 public static SiteProfile Validate(SiteProfile profile)
 {
  if(profile.Logo==null||profile.Address==null||profile.Email==null||profile.Telephone==null||profile.Facebook==null||profile.LinkedIn==null||string.IsNullOrWhiteSpace(profile.Name)||profile.Name.Length>150||profile.Address.Length>500||profile.Email.Length>100||profile.Telephone.Length>50||profile.Facebook.Length>500||profile.LinkedIn.Length>500||profile.Logo.Length>250||(!string.IsNullOrEmpty(profile.Logo)&&!LocalImage(profile.Logo))||(!string.IsNullOrEmpty(profile.Email)&&!System.Net.Mail.MailAddress.TryCreate(profile.Email,out _))||(!string.IsNullOrEmpty(profile.Facebook)&&!Https(profile.Facebook))||(!string.IsNullOrEmpty(profile.LinkedIn)&&!Https(profile.LinkedIn)))throw new UA.Framework.Exceptions.BusinessRuleException("Invalid university profile: use a valid email, HTTPS social links and a local public image path.");return profile;
 }
}
