using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    // Model for companies/businesses
    public class M_Company
    {
        [Key]
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty; // Company name
        public string? tax_id { get; set; } // Tax/VAT ID
        public string? email { get; set; } // Company email
        public string? phone { get; set; } // Company phone
        public string? address { get; set; } // Company address
        public Guid? primary_contact_user_id { get; set; } // Primary contact person
        public bool active { get; set; } = true; // Whether company is active
        public DateTime created_at { get; set; } // When company was created
        public bool monthly_billing_enabled { get; set; } = false; // Whether monthly bundle billing is enabled
    }

    // Model for linking users to companies
    public class M_CompanyUser
    {
        [Key]
        public Guid id { get; set; }
        public Guid company_id { get; set; } // Company ID
        public Guid user_id { get; set; } // User ID
        public CompanyUserRole role { get; set; } // User's role in company
        public DateTime joined_at { get; set; } // When user joined company
    }

    // Roles within a company
    public enum CompanyUserRole
    {
        Employee,      // Regular employee
        Manager,        // Manager with more permissions
        Admin           // Company administrator
    }
}

