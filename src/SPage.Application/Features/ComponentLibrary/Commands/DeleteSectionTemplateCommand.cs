using MediatR;

namespace SPage.Application.Features.ComponentLibrary.Commands;

public sealed record DeleteSectionTemplateCommand(int Id) : IRequest;
