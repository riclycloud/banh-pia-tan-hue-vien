using MediatR;
using SPage.Application.Features.MediaLibrary.DTOs;

namespace SPage.Application.Features.MediaLibrary.Queries;

public sealed record GetAllMediaFilesQuery : IRequest<List<MediaFileDto>>;
