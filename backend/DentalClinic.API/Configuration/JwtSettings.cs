namespace DentalClinic.API.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "DentalClinic.API";

    public string Audience { get; set; } = "DentalClinic.Client";

    public int ExpirationMinutes { get; set; } = 480;
}
