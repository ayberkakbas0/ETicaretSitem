using Microsoft.EntityFrameworkCore;
using ETicaretSitesi;
using ETicaretSitesi.models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ETicaretSitesiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ETicaretSitemDB")));

// Session servisini ekle
builder.Services.AddSession();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'i ekle (Authorization'dan önce olmalý!)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();