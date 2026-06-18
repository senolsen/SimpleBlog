using Blog.Core.Entities;

namespace Blog.Web.Models;

public class HeaderNavViewModel
{
    public SiteSetting Settings { get; set; } = new();
    public List<HeaderCategoryItem> Categories { get; set; } = [];
    public string CurrentPath { get; set; } = "/";
}

public class HeaderCategoryItem
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
}
