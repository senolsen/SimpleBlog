using Blog.Core.Entities;

namespace Blog.Web.Areas.Admin.Models;

public class DashboardViewModel
{
    // Üst Kısım İstatistikleri (Kutular)
    public int TotalPosts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalComments { get; set; }
    public int TotalViews { get; set; }

    // En Çok Okunan 10 Yazı Tablosu İçin
    public List<Post> TopPosts { get; set; } = new List<Post>();

    // Grafik (Chart.js) İçin Kategori ve Yazı Dağılımı
    public List<string> ChartCategoryNames { get; set; } = new List<string>();
    public List<int> ChartPostCounts { get; set; } = new List<int>();
}