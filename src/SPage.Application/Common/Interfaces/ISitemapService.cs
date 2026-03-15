namespace SPage.Application.Common.Interfaces;

public interface ISitemapService
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
