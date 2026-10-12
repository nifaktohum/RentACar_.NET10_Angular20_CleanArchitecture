namespace Infrastructure.Options;

public record class IletimerkeziSettings
{
  public const string SectionName = "Iletimerkezi";

  public string ApiKey { get; init; } = string.Empty;
  public string ApiHash { get; init; } = string.Empty;
  public string Sender { get; init; } = string.Empty;
}

