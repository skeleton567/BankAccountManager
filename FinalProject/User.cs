using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
     public class User
    {
        public string CardNumber { get; set; } = "1234567890123456";
        public string Cvv { get; set; } = "123";
        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddYears(3);
        public string Pin { get; set; } = "1234";
        public List<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>() { new TransactionHistory() };
    }
}
