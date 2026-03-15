using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Email;

namespace SPage.Infrastructure.Services;

public sealed class EmailConfigService : IEmailConfigService
{
    private readonly IApplicationDbContext _context;

    public EmailConfigService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailSendConfig?> GetSendConfigAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailSendConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == 1 && c.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<RecipientEmail>> GetActiveRecipientsAsync(string? groupKey = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RecipientEmails
            .AsNoTracking()
            .Where(r => r.IsActive);

        if (!string.IsNullOrWhiteSpace(groupKey))
            query = query.Where(r => r.GroupKey == groupKey);

        return await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Email)
            .ToListAsync(cancellationToken);
    }
}
