using System.ComponentModel.DataAnnotations;

namespace Product.Reference.Web.Models;

public sealed class SetupDatabaseModel
{
    [Required]
    public string DatabaseType { get; set; } = "Local";

    public string LocalProvider { get; set; } = "LocalDB";

    public string ServerName { get; set; } = @".\SQLEXPRESS";

    [Required]
    public string DatabaseName { get; set; } = "ProductReferenceDB";

    public string AuthenticationType { get; set; } = "Windows";

    public string UserName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool TrustServerCertificate { get; set; } = true;
}
