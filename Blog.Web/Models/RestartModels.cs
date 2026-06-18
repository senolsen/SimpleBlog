namespace Blog.Web.Models;

public static class RestartStrategyNames
{
    public const string Auto = "Auto";
    public const string IisWebConfig = "IisWebConfig";
    public const string LinuxTouchFile = "LinuxTouchFile";
}

public class RestartTriggerResult
{
    public bool Triggered { get; set; }
    public string DetectedPlatform { get; set; } = string.Empty;
    public IReadOnlyList<string> Actions { get; set; } = [];
}
