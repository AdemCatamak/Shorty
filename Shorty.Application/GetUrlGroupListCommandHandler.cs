using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shorty.Db;
using Shorty.Exceptions;

namespace Shorty.Application;

public record GetUrlGroupListCommand(string OwnerId, int PageNumber, int PageSize) : IRequest<GetUrlGroupListCommandResult>;

public record GetUrlGroupListCommandResult(List<GetUrlGroupListCommandResultItem> Data);

public record GetUrlGroupListCommandResultItem(Guid GroupId, string Name, string? Description);

public class GetUrlGroupListCommandValidator : AbstractValidator<GetUrlGroupListCommand>
{
    public GetUrlGroupListCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0);
    }
}

public class GetUrlGroupListCommandHandler : IRequestHandler<GetUrlGroupListCommand, GetUrlGroupListCommandResult>
{
    private readonly AppDbContext _dbContext;

    public GetUrlGroupListCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUrlGroupListCommandResult> Handle(GetUrlGroupListCommand request, CancellationToken cancellationToken)
    {
        var groupCommandResultList = await _dbContext.UrlGroups
            .Where(x => x.OwnerId == request.OwnerId)
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new GetUrlGroupListCommandResultItem(x.Id, x.Name, x.Description))
            .ToListAsync(cancellationToken);

        if (groupCommandResultList.Count == 0)
        {
            throw new NotFoundException($"No UrlGroups found");
        }

        var result = new GetUrlGroupListCommandResult(groupCommandResultList);
        return result;
    }
}