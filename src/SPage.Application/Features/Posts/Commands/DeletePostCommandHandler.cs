using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Posts.Commands;

public sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public DeletePostCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post is null) return false;

        await _slugLookup.RemoveAsync(SlugLookupConstants.TypePost, request.Id, cancellationToken);
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
