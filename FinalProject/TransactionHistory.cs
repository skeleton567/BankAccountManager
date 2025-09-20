using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public class TransactionHistory
    {
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string TransactionType { get; set; } = "Initial Deposit";
        public decimal Amount { get; set; } = 100; 
        public decimal AmountUSD { get; set; } = 100 / Transaction.USDConversion;
        public decimal AmountEUR { get; set; } = 100 / Transaction.EURConversion;
    }
}
