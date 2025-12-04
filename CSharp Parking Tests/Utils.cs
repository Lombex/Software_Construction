using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace CSharpAPI.Tests.Utillities
{
    public class Utils
    {
        /// <summary>
        /// Authenticates a user by sending login credentials to the server and retrieves an authentication token.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> instance used to send the authentication request.</param>
        /// <param name="Username">The username to authenticate with. Defaults to "superadmin" if not specified.</param>
        /// <param name="Password">The password to authenticate with. Defaults to "superpass" if not specified.</param>
        /// <returns>An <see cref="AuthenticationHeaderValue"/> containing the "Bearer" token for the authenticated session.</returns>
        public static async Task<AuthenticationHeaderValue> AuthenticateAsync(HttpClient client, string Username = "superadmin", string Password = "superpass")
        {
            var loginData = new { Username, Password };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginData);
            response.EnsureSuccessStatusCode();
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.token)) throw new InvalidOperationException("Login did not return a token.");
            return new AuthenticationHeaderValue("Bearer", tokenResponse.token);
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
    public class TokenResponse
    {
        public string token { get; set; } = string.Empty;
        public DateTime expiresAt { get; set; }
    }
}
