using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

var app = builder.Build();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Run();
