using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shorty.Db;
using Shorty.Exceptions;

namespace Shorty.Application;

public record GetUrlListCommand(string OwnerId, Guid GroupId, int PageNumber, int PageSize) : IRequest<GetUrlListCommandResult>;

public record GetUrlListCommandResult(List<GetUrlListCommandResultItem> Data);

public record GetUrlListCommandResultItem(Guid UrlId, string OriginalUrl, string ShortUrl, Guid GroupUrlId);

public class GetUrlListCommandValidator : AbstractValidator<GetUrlListCommand>
{
    public GetUrlListCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

public class GetUrlListCommandHandler : IRequestHandler<GetUrlListCommand, GetUrlListCommandResult>
{
    private readonly AppDbContext _dbContext;

    public GetUrlListCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUrlListCommandResult> Handle(GetUrlListCommand request, CancellationToken cancellationToken)
    {
        var groupExist = await _dbContext.UrlGroups.AnyAsync(group => group.Id == request.GroupId && group.OwnerId == request.OwnerId, cancellationToken: cancellationToken);

        if (!groupExist)
        {
            throw new NotFoundException($"Url Group not found");
        }

        var resultItemList = await _dbContext.Urls
            .Where(url => url.UrlGroupId == request.GroupId)
            .OrderBy(url => url.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(url => new GetUrlListCommandResultItem(url.Id, url.OriginalUrl, url.ShortUrl, url.UrlGroupId))
            .ToListAsync(cancellationToken);

        if (resultItemList.Count == 0)
        {
            throw new NotFoundException($"No Urls found");
        }

        return new GetUrlListCommandResult(resultItemList);
    }
}