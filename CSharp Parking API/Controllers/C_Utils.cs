using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }
}
