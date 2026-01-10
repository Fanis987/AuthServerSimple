namespace AuthServerSimple.Application.Options;

/// <summary>
/// Contains passwords for the 3 seeded users
/// </summary>
public class SeedOptions
{
    public const string SeedOptionsSectionName = "SeedOptions";
    
    public bool AddDefaults { get; set; } 
    public string SupportPassword { get; set; } = string.Empty;
    public string DevPassword { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
