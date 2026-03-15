dotnet ef migrations add InitialCreate -p SPage.Infrastructure -s SPage.Web


dotnet ef database update -p SPage.Infrastructure -s SPage.Web

1. Khi bạn thay đổi entity và muốn tạo migration mới
dotnet ef migrations add YourMigrationName -p SPage.Infrastructure -s SPage.Web

2. Apply migration xuống database
dotnet ef database update -p SPage.Infrastructure -s SPage.Web


 Xóa migration cuối (khi vừa tạo sai, chưa apply DB)
 dotnet ef database update UpdateNewFunction -p SPage.Infrastructure -s SPage.Web

====================================
 dotnet ef migrations add ThemDanhMucSP -p SPage.Infrastructure -s SPage.Web
 dotnet ef database update -p SPage.Infrastructure -s SPage.Web