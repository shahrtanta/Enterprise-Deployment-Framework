using Product.Reference.Web.Middleware;
using Product.Reference.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var bootstrap = BootstrapPaths.Create(
    companyName: "Company",
    applicationName: "ProductReference");

builder.Configuration.AddJsonFile(
    bootstrap.RuntimeConfigurationPath,
    optional: true,
    reloadOnChange: true);

builder.Services.AddSingleton(bootstrap);
builder.Services.AddSingleton<RuntimeConfigurationService>();
builder.Services.AddSingleton<DatabaseHealthService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

var port = builder.Configuration.GetValue<int?>("ApplicationSettings:Port") ?? 5080;
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

var app = builder.Build();

DirectoryBootstrap.EnsureCreated(bootstrap);

app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<FirstRunMiddleware>();

app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
