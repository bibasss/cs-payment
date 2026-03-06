namespace PaymentApi.Options;

public class ApiKeySettings
{
    public const string SectionName = "ApiKey";
    public string Key { get; set; } = string.Empty;
}
