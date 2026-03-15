using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPage.Infrastructure.Identity;
using SPage.Application.Common.Interfaces;

namespace SPage.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AccountController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _signIn = signIn;
        _userManager = userManager;
        _emailService = emailService;
    }

    // GET /account/login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST /account/login
    [HttpPost, ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) 
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join("<br/>", errors) });
        }

        var result = await _signIn.PasswordSignInAsync(
            model.Email, model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var redirectUrl = (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
                ? returnUrl 
                : Url.Action("Index", "Dashboard", new { area = "Admin" });
            
            return Json(new { success = true, redirectUrl });
        }

        if (result.IsLockedOut)
        {
            return Json(new { success = false, message = "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau." });
        }

        return Json(new { success = false, message = "Email hoặc mật khẩu không đúng." });
    }

    // POST /account/logout
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    // GET /account/access-denied
    public IActionResult AccessDenied() => View();

    // GET /account/forgot-password
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST /account/forgot-password
    [HttpPost, ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Không tiết lộ user tồn tại hay không
            return View("ForgotPasswordConfirmation");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { email = user.Email, token },
            protocol: Request.Scheme)!;

        await _emailService.SendEmailAsync(
            user.Email!,
            "Đặt lại mật khẩu - SPage",
            $"""
            <p>Xin chào,</p>
            <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản trên SPage.</p>
            <p>Nhấn vào liên kết bên dưới để đặt lại mật khẩu:</p>
            <p><a href="{callbackUrl}">{callbackUrl}</a></p>
            <p>Nếu bạn không yêu cầu thao tác này, vui lòng bỏ qua email này.</p>
            """);

        ViewBag.ResetLink = callbackUrl; // vẫn hiển thị trong dev cho tiện debug
        return View("ForgotPasswordConfirmation");
    }

    // GET /account/reset-password
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return BadRequest();

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };
        return View(model);
    }

    // POST /account/reset-password
    [HttpPost, ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET /account/change-password
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST /account/change-password
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(ChangePassword));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
}

public sealed class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email.")]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class ForgotPasswordViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email.")]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordViewModel
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Token { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
