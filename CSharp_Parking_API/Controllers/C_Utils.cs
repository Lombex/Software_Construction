using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace CSharpAPI.Controllers.Utils
{
    public static class C_Utils
    {
        // Validate email format
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch { return false; }
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 7 && phoneNumber.Length <= 15;
        }

        // Hash password using BCrypt (secure, with salt)
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            
            // BCrypt automatically generates a salt and includes it in the hash
            // Work factor of 12 provides good security (2^12 = 4096 iterations)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // Verify password - supports both BCrypt (new) and SHA256 (legacy) for backward compatibility
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            // Check if it's a BCrypt hash (starts with $2a$, $2b$, or $2y$)
            if (hashedPassword.StartsWith("$2") && hashedPassword.Length > 20)
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                catch
                {
                    return false;
                }
            }
            
            // Legacy SHA256 support for existing passwords (backward compatibility)
            // This allows existing users to login, but their password will be rehashed on next login
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                var hashString = Convert.ToBase64String(hash);
                return StringComparer.Ordinal.Compare(hashString, hashedPassword) == 0;
            }
        }

        // Check if a hash is using the old SHA256 method (for migration purposes)
        public static bool IsLegacyHash(string hashedPassword)
        {
            return !hashedPassword.StartsWith("$2") || hashedPassword.Length <= 20;
        }
    }
}
