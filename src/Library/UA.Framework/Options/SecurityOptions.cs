namespace UA.Framework.Options;
public sealed class SecurityOptions
{
    public const string Section = "Security";
    public int LoginFailureLimit { get; set; } = 5;
    public int LockMinutes { get; set; } = 15;
    public int SessionMinutes { get; set; } = 30;
}
