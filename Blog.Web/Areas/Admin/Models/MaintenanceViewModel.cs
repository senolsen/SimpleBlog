using System.ComponentModel.DataAnnotations;

namespace Blog.Web.Areas.Admin.Models;

public class MaintenanceViewModel
{
    public bool IsMaintenanceMode { get; set; }

    [MaxLength(500, ErrorMessage = "Bakım mesajı en fazla 500 karakter olabilir.")]
    public string? MaintenanceMessage { get; set; }

    public string SiteTitle { get; set; } = string.Empty;
}
