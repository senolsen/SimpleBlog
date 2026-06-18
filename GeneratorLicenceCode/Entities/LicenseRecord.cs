namespace GeneratorLicenceCode.Entities;

public class LicenseRecord
{
    public int Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
