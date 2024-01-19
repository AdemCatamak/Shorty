namespace Shorty.Api.Controllers.Contracts;

public record GetUrlGroupListRequest(int PageNumber, int PageSize);

public record GetUrlGroupListResponse(List<GetUrlGroupResponse> Data);

public record GetUrlGroupResponse(Guid GroupId, string Name, string? Description);