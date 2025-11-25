namespace SupportToolsServerDbDataSeeding.Models;

public sealed class ApiKeyModel
{
    //აპის გასაღები
    public required string ApiKey { get; set; }

    //IP მისამართი, საიდანაც ამ აპის გასაღების საშუალებით შეძლებენ შემოსვლას
    public required string RemoteIpAddress { get; set; }
}