using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    // Model for user balance/saldo
    public class M_UserBalance
    {
        [Key]
        public Guid id { get; set; }
        public Guid user_id { get; set; } // User who owns this balance
        public decimal balance { get; set; } // Current balance amount
        public string currency { get; set; } = "EUR"; // Currency code
        public DateTime last_updated { get; set; } // Last time balance was updated
        public DateTime created_at { get; set; } // When balance record was created
    }

    // Model for balance transactions (history)
    public class M_BalanceTransaction
    {
        [Key]
        public Guid id { get; set; }
        public Guid user_id { get; set; } // User who made the transaction
        public Guid? balance_id { get; set; } // Related balance record
        public decimal amount { get; set; } // Transaction amount (positive = credit, negative = debit)
        public string currency { get; set; } = "EUR";
        public TransactionType type { get; set; } // Type of transaction
        public string? description { get; set; } // Transaction description
        public Guid? payment_id { get; set; } // Related payment if applicable
        public Guid? session_id { get; set; } // Related session if applicable
        public DateTime created_at { get; set; } // When transaction occurred
    }

    // Types of balance transactions
    public enum TransactionType
    {
        Credit,         // Money added to balance
        Debit,          // Money deducted from balance
        Payment,        // Payment made from balance
        Refund,         // Refund added to balance
        Deposit,        // Initial deposit
        Withdrawal      // Withdrawal from balance
    }
}

