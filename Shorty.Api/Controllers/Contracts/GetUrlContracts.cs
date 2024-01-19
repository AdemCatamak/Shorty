namespace Shorty.Api.Controllers.Contracts;

public record GetUrlListRequest(int PageNumber, int PageSize);

public record GetUrlListResponse(List<GetUrlResponse> Data);

public record GetUrlResponse(string OriginalUrl, string ShortUrl);