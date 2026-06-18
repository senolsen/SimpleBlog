using Blog.Core.Entities;

namespace Blog.Web.Helpers;

public record SocialLinkItem(string Name, string? Url, string IconClass);

public static class SocialLinkBuilder
{
    public static IReadOnlyList<SocialLinkItem> FromSettings(SiteSetting settings) =>
    [
        new("Facebook", settings.FacebookUrl, "bi-facebook"),
        new("Instagram", settings.InstagramUrl, "bi-instagram"),
        new("LinkedIn", settings.LinkedinUrl, "bi-linkedin"),
        new("GitHub", settings.GithubUrl, "bi-github")
    ];
}
