using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shorty.Core;
using Shorty.Db;
using Shorty.Exceptions;

namespace Shorty.Application;

public record CreateUrlGroupCommand(string Name, string? Description, string OwnerId) : IRequest<CreateUrlGroupCommandResult>;

public record CreateUrlGroupCommandResult(Guid GroupId);

public class CreateUrlGroupCommandValidator : AbstractValidator<CreateUrlGroupCommand>
{
    public CreateUrlGroupCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.Description)
            .MaximumLength(500);

        RuleFor(c => c.OwnerId)
            .NotEmpty();
    }
}

public class CreateUrlGroupCommandHandler : IRequestHandler<CreateUrlGroupCommand, CreateUrlGroupCommandResult>
{
    private readonly AppDbContext _dbContext;

    public CreateUrlGroupCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateUrlGroupCommandResult> Handle(CreateUrlGroupCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.ToLowerInvariant().Trim();
        var exist = await _dbContext.UrlGroups.AnyAsync(u => u.Name == normalizedName, cancellationToken);

        if (exist)
        {
            throw new BusinessException("Url group with this name could not be created.");
        }

        var urlGroup = UrlGroup.Create(request.Name, request.Description, request.OwnerId);

        await _dbContext.AddAsync(urlGroup, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new CreateUrlGroupCommandResult(urlGroup.Id);
        return result;
    }
}