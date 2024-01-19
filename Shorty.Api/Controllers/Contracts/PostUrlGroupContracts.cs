namespace Shorty.Api.Controllers.Contracts;

public record PostUrlGroupRequest(string Name, string? Description, string OwnerId);

public record PostUrlGroupResponse(Guid GroupId);