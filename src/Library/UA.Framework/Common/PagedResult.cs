namespace UA.Framework.Common;
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
