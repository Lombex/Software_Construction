namespace CSharpAPI.Models
{
    public class Model_Users
    {
        public enum UserRole
        {
            Admin,
            User
        }

        public Guid id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public UserRole role { get; set; }
        public DateTime created_at { get; set; }
        public DateTime birth_year { get; set; }
        public bool active { get; set; }
    }
}
