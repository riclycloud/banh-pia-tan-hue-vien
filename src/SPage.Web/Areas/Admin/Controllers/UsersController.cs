using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Infrastructure.Identity;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var models = new List<UserListItemViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            models.Add(new UserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName ?? "",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = roles.ToList()
            });
        }

        ViewData["Title"] = "Người dùng";
        return View(models);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            IsActive = user.IsActive,
            IsAdmin = roles.Contains("Admin"),
            IsContentManager = roles.Contains("ContentManager")
        };

        ViewData["Title"] = "Sửa người dùng";
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa người dùng";
            return View(model);
        }

        user.FullName = model.FullName;
        user.IsActive = model.IsActive;

        await _userManager.UpdateAsync(user);

        // Cập nhật roles đơn giản cho 2 role chính
        var roles = await _userManager.GetRolesAsync(user);

        async Task EnsureRoleAsync(string roleName, bool shouldHave)
        {
            var hasRole = roles.Contains(roleName);
            if (shouldHave && !hasRole)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                await _userManager.AddToRoleAsync(user, roleName);
            }
            else if (!shouldHave && hasRole)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
        }

        await EnsureRoleAsync("Admin", model.IsAdmin);
        await EnsureRoleAsync("ContentManager", model.IsContentManager);

        TempData["Success"] = "Đã cập nhật người dùng.";
        return RedirectToAction(nameof(Index));
    }
}

public sealed class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}

public sealed class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool IsAdmin { get; set; }
    public bool IsContentManager { get; set; }
}

