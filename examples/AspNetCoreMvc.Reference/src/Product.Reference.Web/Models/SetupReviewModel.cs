namespace Product.Reference.Web.Models;

public sealed record SetupReviewModel(
    string DataRoot,
    string RuntimeConfigurationPath,
    int Port,
    string DatabaseType,
    string DatabaseName,
    string ServerName,
    string AuthenticationType);
