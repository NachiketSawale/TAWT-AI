namespace TawtAi.Api.Settings;

public class AzureDevOpsSettings
{
    public const string SectionName = "AzureDevOps";

    public string IdentityBaseUrl { get; set; } = "https://vssps.dev.azure.com/";
    public string OrganizationBaseUrl { get; set; } = "https://dev.azure.com/";
    public string ApiVersion { get; set; } = "7.1";
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = "master";
    public string ModulesScopePath { get; set; } = "/frontend.ngjs";
}
