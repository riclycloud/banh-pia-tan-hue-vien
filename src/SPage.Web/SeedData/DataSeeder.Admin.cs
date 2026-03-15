using Microsoft.AspNetCore.Identity;
using SPage.Infrastructure.Identity;

namespace SPage.Web.SeedData;

internal static partial class DataSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        const string adminRole           = "Admin";
        const string contentManagerRole  = "ContentManager";
        const string adminEmail          = "admin@spage.dev";
        const string adminPassword       = "Admin@123456";

        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        if (!await roleManager.RoleExistsAsync(contentManagerRole))
            await roleManager.CreateAsync(new IdentityRole(contentManagerRole));

        // Luôn đảm bảo tồn tại tài khoản admin với mật khẩu chuẩn
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName       = adminEmail,
                Email          = adminEmail,
                FullName       = "Administrator",
                EmailConfirmed = true,
                IsActive       = true
            };
            await userManager.CreateAsync(user);
        }

        // Đảm bảo user kích hoạt & email xác nhận
        user.EmailConfirmed = true;
        user.IsActive       = true;
        await userManager.UpdateAsync(user);

        // Reset mật khẩu về giá trị mặc định
        if (await userManager.HasPasswordAsync(user))
            await userManager.RemovePasswordAsync(user);
        await userManager.AddPasswordAsync(user, adminPassword);

        // Đảm bảo user có role Admin
        if (!await userManager.IsInRoleAsync(user, adminRole))
            await userManager.AddToRoleAsync(user, adminRole);
    }
}
