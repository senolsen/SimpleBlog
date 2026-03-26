namespace Blog.Web.Areas.Admin.Models
{
    // 3. Listeleme (Index) Sayfası İçin
    public class UserListViewModel
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
