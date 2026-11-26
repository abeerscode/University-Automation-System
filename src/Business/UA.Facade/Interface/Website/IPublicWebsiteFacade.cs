using UA.Facade.DTO.Website;
namespace UA.Facade.Interface.Website;
public interface IPublicWebsiteFacade
{
 Task<WebsiteHome> HomeAsync();
 Task<IReadOnlyList<PublicFaculty>> FacultyAsync();
 Task<PublicPage?> PageAsync(string slug);
 Task<IReadOnlyList<PageVersionRow>> HistoryAsync();
 Task DraftAsync(string slug,string title,int? departmentId,string content);
 Task ApproveAsync(int versionId);
}
