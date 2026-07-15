using Microsoft.AspNetCore.Mvc;
using Product.Reference.Web.Models;
using Product.Reference.Web.Services;

namespace Product.Reference.Web.Controllers;

[Route("Setup")]
public sealed class SetupController : Controller
{
    private const int DefaultPort = 5080;

    private readonly BootstrapPaths _paths;
    private readonly RuntimeConfigurationService _configuration;
    private readonly DatabaseHealthService _databaseHealth;

    public SetupController(
        BootstrapPaths paths,
        RuntimeConfigurationService configuration,
        DatabaseHealthService databaseHealth)
    {
        _paths = paths;
        _configuration = configuration;
        _databaseHealth = databaseHealth;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new SetupDatabaseModel());
    }

    [HttpPost("TestConnection")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnection(
        SetupDatabaseModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Review the required database fields."
            });
        }

        try
        {
            var connection = ConnectionStringBuilderService.Build(model, _paths);
            var result = await _databaseHealth.TestAsync(
                connection,
                cancellationToken);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost("Review")]
    [ValidateAntiForgeryToken]
    public IActionResult Review(SetupDatabaseModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        TempData["SetupModel"] = System.Text.Json.JsonSerializer.Serialize(model);

        return View(new SetupReviewModel(
            _paths.DataRoot,
            _paths.RuntimeConfigurationPath,
            DefaultPort,
            model.DatabaseType,
            model.DatabaseName,
            model.ServerName,
            model.AuthenticationType));
    }

    [HttpPost("Complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        CancellationToken cancellationToken)
    {
        var json = TempData["SetupModel"] as string;
        if (string.IsNullOrWhiteSpace(json))
        {
            return RedirectToAction(nameof(Index));
        }

        var model = System.Text.Json.JsonSerializer.Deserialize<SetupDatabaseModel>(json);
        if (model is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var connectionString = ConnectionStringBuilderService.Build(model, _paths);
        var test = await _databaseHealth.TestAsync(
            connectionString,
            cancellationToken);

        if (!test.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                "Setup was not completed because the database connection failed.");

            TempData["SetupModel"] = json;

            return View("Review", new SetupReviewModel(
                _paths.DataRoot,
                _paths.RuntimeConfigurationPath,
                DefaultPort,
                model.DatabaseType,
                model.DatabaseName,
                model.ServerName,
                model.AuthenticationType));
        }

        await _configuration.SaveAsync(
            model,
            DefaultPort,
            cancellationToken);

        _configuration.MarkSetupComplete();

        return RedirectToAction("Index", "Home");
    }
}
