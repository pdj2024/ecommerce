namespace WebShared.Models
{
    public class UserAccountPolicy
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public string? UserPolicy { get; set; }
        public bool IsEnabled { get; set; }
    }
}
