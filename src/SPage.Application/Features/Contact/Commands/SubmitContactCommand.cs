using System.Text.Json;
using FluentValidation;
using MediatR;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Contact;

namespace SPage.Application.Features.Contact.Commands;

public sealed record SubmitContactCommand(
    Dictionary<string, string> Fields,
    string? Subject = null,
    string? SenderIp = null
) : IRequest<bool>;

public sealed class SubmitContactCommandValidator : AbstractValidator<SubmitContactCommand>
{
    public SubmitContactCommandValidator()
    {
        RuleFor(x => x.Fields)
            .NotNull().WithMessage("Dữ liệu form không được rỗng.")
            .Must(f => f.Count > 0).WithMessage("Vui lòng điền ít nhất một trường thông tin.");
    }
}

public sealed class SubmitContactCommandHandler : IRequestHandler<SubmitContactCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public SubmitContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(SubmitContactCommand request, CancellationToken cancellationToken)
    {
        var f = request.Fields;

        var subject = request.Subject?.Trim();
        if (string.IsNullOrEmpty(subject))
            subject = f.GetValueOrDefault("subject")?.Trim();

        var message = new ContactMessage
        {
            Subject     = subject,
            SenderName  = f.GetValueOrDefault("name")
                       ?? f.GetValueOrDefault("fullName")
                       ?? f.GetValueOrDefault("hoTen"),
            SenderEmail = f.GetValueOrDefault("email"),
            SenderPhone = f.GetValueOrDefault("phone")
                       ?? f.GetValueOrDefault("soDienThoai"),
            SenderIp    = request.SenderIp,
            Status      = ContactMessageStatus.New,
            DataJson    = JsonSerializer.Serialize(f,
                new JsonSerializerOptions { WriteIndented = false }),
        };

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
