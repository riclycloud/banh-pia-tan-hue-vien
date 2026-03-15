namespace SPage.Application.Features.MediaLibrary.DTOs;

public sealed record MediaFileDto(
    int             Id,
    string          FileName,
    string          Url,
    long            FileSize,
    string          MimeType,
    string?         AltText,
    DateTimeOffset  CreatedAt);
