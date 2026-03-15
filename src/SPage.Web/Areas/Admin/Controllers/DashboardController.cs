using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class DashboardController : Controller
{
    private readonly IApplicationDbContext _context;

    public DashboardController(IApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var stats = new DashboardStats
        {
            TotalPosts = await _context.Posts.IgnoreQueryFilters().CountAsync(),
            TotalProducts = await _context.Products.CountAsync(),
            TotalPages = await _context.DynamicPages.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync()
        };

        ViewData["Title"] = "Dashboard";
        return View(stats);
    }
}

public sealed class DashboardStats
{
    public int TotalPosts { get; set; }
    public int TotalProducts { get; set; }
    public int TotalPages { get; set; }
    public int TotalCategories { get; set; }
}
