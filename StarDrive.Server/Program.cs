using Microsoft.AspNetCore.Http.Connections;
using StarDrive.Server;
using StarDrive.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR()
    .AddHubOptions<DriveHub>(options =>
{
    options.MaximumParallelInvocationsPerClient = 10;
    options.MaximumReceiveMessageSize = null; // 2147483647;  // set to null for unlimited? 
    options.EnableDetailedErrors = true;
});
builder.Services.AddSingleton<StarDriveService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapHub<DriveHub>("/drivehub",options =>
{
    options.Transports = HttpTransportType.WebSockets; // don't allow long-polling; 
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
