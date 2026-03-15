using MediatR;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record UpdateVideoCategoryCommand(
    int     Id,
    string  Name,
    string? Description,
    int     DisplayOrder,
    bool    IsActive,
    string? Slug = null) : IRequest;
