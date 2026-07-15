using Microsoft.AspNetCore.Mvc;
using Product.Reference.Web.Services;

namespace Product.Reference.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly BootstrapPaths _paths;

    public HomeController(BootstrapPaths paths)
    {
        _paths = paths;
    }

    public IActionResult Index()
    {
        ViewData["DataRoot"] = _paths.DataRoot;
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
