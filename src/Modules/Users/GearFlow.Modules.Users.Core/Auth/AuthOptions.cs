using System.ComponentModel.DataAnnotations;

namespace GearFlow.Modules.Users.Core.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "auth";

    [Required]
    public string Issuer { get; set; } = default!;
    [Required]
    public string Audience { get; set; } = default!;
    [Required]
    public string SigningKey { get; set; } = default!;
    [Required]
    public TimeSpan Expiry { get; set; }
    [Required]
    public TimeSpan ExpiryRefreshToken { get; set; }
}
