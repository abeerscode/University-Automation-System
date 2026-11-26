namespace UA.Facade.DTO.Website;
public record PublicDepartment(int Id,string Code,string Name,string? Program,int? Credits,int? Semesters);
public record PublicFaculty(string Name,string Department,string Designation,string? Qualifications);
public record PublicNotice(int Id,string Title,string Body,DateTime Published,string Department);
public record PublicPage(int Id,int? DepartmentId,string Slug,string Title,string Content,int Version);
public record PageVersionRow(int PageId,int VersionId,int Version,string Slug,string Title,int? DepartmentId,string Content,int Author,int? Approver,DateTime Changed);
public record WebsiteHome(IReadOnlyList<PublicDepartment> Departments,IReadOnlyList<PublicNotice> Notices,IReadOnlyList<PublicPage> Pages);
