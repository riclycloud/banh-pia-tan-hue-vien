using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPage.Application.Features.Categories.Queries;
using SPage.Application.Features.Posts.Commands;
using SPage.Application.Features.Posts.Queries;
using SPage.Application.Features.Tags.Queries;
using SPage.Domain.Enums;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class PostsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public PostsController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    // GET /admin/posts
    public async Task<IActionResult> Index(int page = 1, string? q = null, PostStatus? status = null)
    {
        var result = await _mediator.Send(new GetAllPostsQuery(page, 20, q, status));
        ViewData["Title"] = "Bài viết";
        ViewBag.SearchTerm = q;
        ViewBag.Status = status;
        return View(result);
    }

    // GET /admin/posts/create
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm bài viết";
        await PopulateViewBag();

        var model = new PostFormModel();
        if (TempData["SeoAiDraft"] is string json)
        {
            try
            {
                var draft = System.Text.Json.JsonSerializer.Deserialize<SeoAiDraftPayload>(json);
                if (draft != null)
                {
                    model.Title = draft.Title ?? "";
                    model.Content = draft.Content ?? "";
                    model.Excerpt = draft.Excerpt;
                    model.MetaTitle = draft.MetaTitle;
                    model.MetaDescription = draft.MetaDescription;
                    ViewData["FromSeoAi"] = true;
                }
            }
            catch { /* ignore */ }
        }
        return View(model);
    }

    private sealed class SeoAiDraftPayload
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Excerpt { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }

    // POST /admin/posts/create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBag();
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.MetaTitle))
        {
            model.MetaTitle = model.Title;
        }

        var id = await _mediator.Send(new CreatePostCommand(
            model.Title,
            model.Content,
            model.Excerpt,
            model.CategoryId,
            model.TagIds ?? [],
            model.MetaTitle,
            model.MetaDescription,
            model.FeaturedImageUrl,
            model.FeaturedImageAlt,
            model.IsIndexable,
            model.SchemaType,
            model.Slug,
            model.Status));

        TempData["Success"] = "Bài viết đã được tạo thành công.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // GET /admin/posts/edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _mediator.Send(new GetPostByIdQuery(id));
        if (post is null) return NotFound();

        ViewData["Title"] = "Sửa bài viết";
        await PopulateViewBag(post.TagIds);

        var model = new PostFormModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Excerpt = post.Excerpt,
            CategoryId = post.CategoryId,
            TagIds = post.TagIds,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            CanonicalUrl = post.CanonicalUrl,
            FeaturedImageUrl = post.FeaturedImageUrl,
            FeaturedImageAlt = post.FeaturedImageAlt,
            IsIndexable = post.IsIndexable,
            SchemaType = post.SchemaType,
            Status = post.Status
        };

        return View(model);
    }

    // POST /admin/posts/edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostFormModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateViewBag(model.TagIds);
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.MetaTitle))
        {
            model.MetaTitle = model.Title;
        }

        var success = await _mediator.Send(new UpdatePostCommand(
            model.Id,
            model.Title,
            model.Content,
            model.Excerpt,
            model.CategoryId,
            model.TagIds ?? [],
            model.MetaTitle,
            model.MetaDescription,
            model.CanonicalUrl,
            model.FeaturedImageUrl,
            model.FeaturedImageAlt,
            model.IsIndexable,
            model.SchemaType,
            model.Status,
            model.Slug));

        if (!success) return NotFound();

        TempData["Success"] = "Bài viết đã được cập nhật.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // POST /admin/posts/delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeletePostCommand(id));
        var message = "Bài viết đã được xóa.";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, message });
        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }



    private async Task PopulateViewBag(List<int>? selectedTagIds = null)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery());
        var tags = await _mediator.Send(new GetTagsQuery());

        ViewBag.Categories = categories.Select(c =>
            new SelectListItem(c.Name, c.Id.ToString())).ToList();

        ViewBag.Tags = tags.Select(t =>
            new SelectListItem(
                t.Name,
                t.Id.ToString(),
                selectedTagIds?.Contains(t.Id) == true)).ToList();
    }
}

/// <summary>ViewModel cho form tạo/sửa bài viết.</summary>
public sealed class PostFormModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tiêu đề không được để trống.")]
    [System.ComponentModel.DataAnnotations.MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Slug tùy chỉnh. Nếu để trống sẽ tự sinh từ tiêu đề.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(300)]
    public string? Slug { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Nội dung không được để trống.")]
    public string Content { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Chọn chuyên mục.")]
    public int CategoryId { get; set; }

    public List<int>? TagIds { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(70)]
    public string? MetaTitle { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(160)]
    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? FeaturedImageAlt { get; set; }
    public bool IsIndexable { get; set; } = true;
    public string SchemaType { get; set; } = "Article";
    public PostStatus Status { get; set; } = PostStatus.Draft;
}
