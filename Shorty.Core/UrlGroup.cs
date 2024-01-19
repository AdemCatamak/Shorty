namespace Shorty.Core;

public class UrlGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    public string OwnerId { get; private set; }

    private UrlGroup(Guid id, string name, string? description, string ownerId)
    {
        Id = id;
        Name = name;
        Description = description;
        OwnerId = ownerId;
    }

    public static UrlGroup Create(string name, string? description, string ownerId)
    {
        return new(Guid.NewGuid(), name, description, ownerId);
    }
}