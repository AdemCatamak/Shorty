using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Shorty.Application.Services;
using Shorty.Core;
using Shorty.Db;
using Shorty.Exceptions;

namespace Shorty.Application;

public record CreateUrlCommand(string OriginalUrl, string? ShortUrl, Guid GroupId, string OwnerId) : IRequest<CreateUrlCommandResult>;

public record CreateUrlCommandResult(Guid UrlId);

public class CreateUrlCommandValidator : AbstractValidator<CreateUrlCommand>
{
    public CreateUrlCommandValidator()
    {
        RuleFor(c => c.OriginalUrl)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(c => c.ShortUrl)
            .MaximumLength(50);

        RuleFor(c => c.GroupId)
            .NotEmpty();

        RuleFor(c => c.OwnerId)
            .NotEmpty();
    }
}

public class CreateUrlCommandHandler : IRequestHandler<CreateUrlCommand, CreateUrlCommandResult>
{
    private readonly AppDbContext _dbContext;
    private readonly IRandomGenerator _randomGenerator;

    public CreateUrlCommandHandler(AppDbContext dbContext, IRandomGenerator randomGenerator)
    {
        _dbContext = dbContext;
        _randomGenerator = randomGenerator;
    }

    public async Task<CreateUrlCommandResult> Handle(CreateUrlCommand request, CancellationToken cancellationToken)
    {
        var urlGroup = await _dbContext.UrlGroups
            .FirstOrDefaultAsync(x => x.Id == request.GroupId && x.OwnerId == request.OwnerId,
                cancellationToken);

        if (urlGroup == null)
        {
            throw new NotFoundException("Url group not found.");
        }

        string shortenedUrl;

        if (!string.IsNullOrEmpty(request.ShortUrl))
        {
            shortenedUrl = request.ShortUrl;
            var exist = await _dbContext.Urls.AnyAsync(u => u.ShortUrl == request.ShortUrl && u.UrlGroupId == urlGroup.Id, cancellationToken);
            if (exist)
            {
                throw new BusinessException("Short url already exists.");
            }
        }
        else
        {
            shortenedUrl = await Policy.Handle<BusinessException>()
                .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(100))
                .ExecuteAsync(async (token) =>
                        {
                            string sUrl = _randomGenerator.Generate();

                            var exist = await _dbContext.Urls.AnyAsync(u => u.ShortUrl == sUrl && u.UrlGroupId == urlGroup.Id, cancellationToken);
                            if (exist)
                            {
                                throw new BusinessException("Unique short url could not generated. Please try again later.");
                            }

                            return sUrl;
                        },
                    cancellationToken);
        }

        var url = Url.Create(request.OriginalUrl, shortenedUrl, urlGroup);
        await _dbContext.AddAsync(url, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new CreateUrlCommandResult(url.Id);
        return result;
    }
}