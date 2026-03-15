using MediatR;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record CreateVideoCategoryCommand(
    string  Name,
    string? Description   = null,
    int     DisplayOrder  = 0,
    bool    IsActive      = true,
    string? Slug          = null) : IRequest<int>;
