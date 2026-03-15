using FluentValidation;

namespace SPage.Application.Features.Posts.Commands;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung không được để trống.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Vui lòng chọn chuyên mục.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(70).WithMessage("Meta Title không được vượt quá 70 ký tự.")
            .When(x => x.MetaTitle is not null);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).WithMessage("Meta Description không được vượt quá 160 ký tự.")
            .When(x => x.MetaDescription is not null);

        RuleFor(x => x.SchemaType)
            .Must(t => new[] { "Article", "NewsArticle", "BlogPosting" }.Contains(t))
            .WithMessage("SchemaType phải là Article, NewsArticle hoặc BlogPosting.");
    }
}
