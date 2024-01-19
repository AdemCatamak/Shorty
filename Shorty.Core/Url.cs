namespace Shorty.Core;

public class Url
{
    public Guid Id { get; private set; }
    public string OriginalUrl { get; private set; }
    public string ShortUrl { get; private set; }
    public Guid UrlGroupId { get; private set; }

    private Url(Guid id, string originalUrl, string shortUrl, Guid urlGroupId)
    {
        Id = id;
        OriginalUrl = originalUrl;
        ShortUrl = shortUrl;
        UrlGroupId = urlGroupId;
    }

    public static Url Create(string originalUrl, string shortenedUrl, UrlGroup urlGroup)
    {
        return new(Guid.NewGuid(), originalUrl, shortenedUrl, urlGroup.Id);
    }
}