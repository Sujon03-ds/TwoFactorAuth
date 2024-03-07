using System.ComponentModel.DataAnnotations;

namespace TwoFactorAuth.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string? SecretKey { get; set; }
    }
}
