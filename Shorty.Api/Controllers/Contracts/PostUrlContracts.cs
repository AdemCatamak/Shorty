namespace Shorty.Api.Controllers.Contracts;

public record PostUrlRequest(string OriginalUrl, string? ShortUrl, Guid GroupUrlId, string OwnerId);

public record PostUrlResponse(Guid UrlId);