using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualBasic;

namespace FinalProject
{
    public delegate void TransactionDelegate(string mesage, string type = "transaction");

    static class Transaction
    {
        public static readonly decimal EURConversion = 2.85m;

        public static readonly decimal USDConversion = 2.6m;

        public static event TransactionDelegate OnTransaction;

        public static void CheckDeposit(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().Amount} GEl");

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountUSD} USD");

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountEUR} EUR");

            TriggerTransactionEvent("User checked their balance.");
        }

        public static void Withdraw(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);
            var lastTransaction = user.TransactionHistories.Last();
            Console.Write("Enter amount to withdraw: ");
            var input = Console.ReadLine() ?? string.Empty;
            var amount = decimal.TryParse(input, out decimal parsedAmount) ? parsedAmount : 0;
            if (amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                return;
            }
            if (amount > lastTransaction.Amount)
            {
                Console.WriteLine("Insufficient funds.");
                return;
            }
            var newBalance = lastTransaction.Amount - amount;

            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = newBalance,
                AmountUSD = lastTransaction.AmountUSD,
                AmountEUR = lastTransaction.AmountEUR,
                TransactionType = "Withdraw"
            };
            user.TransactionHistories.Add(newTransaction);
            SaveNewUser(user);
            Console.WriteLine($"Successfully withdrew {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");

            TriggerTransactionEvent($"User withdrawn {amount}. New balance is: {newBalance} GEL");
        }

        public static void Deposit(User newUser, string argumentAmount)
        {
            var amount = decimal.TryParse(argumentAmount, out decimal parsedAmount) ? parsedAmount : 0;

            if (amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                return;
            }

            var user = FindOrCreateUserJson(newUser);
            var lastTransaction = user.TransactionHistories.Last();
            var newBalance = lastTransaction.Amount + amount;

            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = newBalance,
                AmountUSD = lastTransaction.AmountUSD,
                AmountEUR = lastTransaction.AmountEUR,
                TransactionType = "Deposit"
            };
            user.TransactionHistories.Add(newTransaction);
            SaveNewUser(user);
            Console.WriteLine($"Successfully deposited {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");

            TriggerTransactionEvent($"User deposited {amount}. New balance is: {newBalance} GEL");
        }

        public static void getLastFiveTransactions(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);
            var lastSixTransactions = user.TransactionHistories
                .OrderByDescending(t => t.TransactionDate)
                .Take(6)
                .ToList();

            if (lastSixTransactions.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                return;
            }
            Console.WriteLine("Last 5 transactions:");

            var maxCount = Math.Min(lastSixTransactions.Count, 6);

            for (int i = 0; i < maxCount; i++)
            {
                var transaction = lastSixTransactions.ElementAt(i);
          
                var previousTransaction = lastSixTransactions.ElementAtOrDefault(i + 1);

                if(transaction.TransactionType.Contains("Conversion") && previousTransaction != null)
                {
                   
                    var prevGEL = previousTransaction.Amount;
                    var currGEL = transaction.Amount;
                    var gelChangeConv = prevGEL - currGEL;

                    var otherCurrencyAmount = transaction.TransactionType.Contains("USD") ? transaction.AmountUSD : transaction.AmountEUR;
                    var prevOtherCurrencyAmount = previousTransaction.TransactionType.Contains("USD") ? previousTransaction.AmountUSD : previousTransaction.AmountEUR;
                    var otherCurrencyChange = otherCurrencyAmount - prevOtherCurrencyAmount;

                    Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} {gelChangeConv} GEL converted to {otherCurrencyChange}");
                    continue;
                }

                decimal gelChange;

                if (previousTransaction != null && (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Withdraw"))
                {
                    gelChange = previousTransaction.Amount - transaction.Amount;
    
                }
                else
                {
                    gelChange = transaction.Amount;
                }

                Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} of {Math.Abs(gelChange)} GEL");
            }

            TriggerTransactionEvent($"User checked last 5 transactions");
        }

        public static void ChangePin(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);
            Console.Write("Enter your current PIN: ");
            var currentPin = Console.ReadLine() ?? string.Empty;
            if (currentPin != user.Pin)
            {
                Console.WriteLine("Incorrect PIN.");
                return;
            }
            Console.Write("Enter your new 4-digit PIN: ");
            var newPin = Console.ReadLine() ?? string.Empty;
            if (newPin.Length != 4 || !int.TryParse(newPin, out _))
            {
                Console.WriteLine("Invalid PIN. It must be 4 digits.");
                return;
            }
            user.Pin = newPin;
            SaveNewUser(user);
            Console.WriteLine("PIN successfully changed.");

            TriggerTransactionEvent($"User changed pin");
        }

        public static void MoneyConversion(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);
            var lastTransaction = user.TransactionHistories.Last();
            Console.WriteLine("Choose currency to convert to:");
            Console.WriteLine("1. USD");
            Console.WriteLine("2. EUR");
            Console.Write("Select an option (1-2): ");
            var choice = Console.ReadLine() ?? string.Empty;
            switch (choice)
            {
                case "1":
                    convertAmount(newUser, "USD");
                    break;
                case "2":
                    convertAmount(newUser, "EUR");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            TriggerTransactionEvent($"User converted money");
        }

        public static User FindOrCreateUserJson(User newUser)
        {
            try
            {
                var jsonString = File.ReadAllText("user.json");
                var user = JsonSerializer.Deserialize<User>(jsonString);

                if(user == null || user.CardNumber != newUser.CardNumber)
                {
                    return SaveNewUser(newUser); 
                }

                return user;
            }
            catch (FileNotFoundException)
            {
                return SaveNewUser(newUser);
            }
            catch (Exception e)
            {
                TriggerTransactionEvent(e.Message, "error");
                return SaveNewUser(newUser);
            }
        }

        public static User SaveNewUser(User newUser)
        {
            var jsonString = JsonSerializer.Serialize(newUser);
            File.WriteAllText("user.json", jsonString);
            var user = JsonSerializer.Deserialize<User>(jsonString);

            return user!;
        }

        private static void convertAmount(User newUser ,string currency = "USD")
        {
            Console.Write("Enter amount in GEL to convert: ");
            var input = Console.ReadLine() ?? string.Empty;
            var amount = decimal.TryParse(input, out decimal parsedAmount) ? parsedAmount : 0;
            var lastTransaction = newUser.TransactionHistories.Last();

            if (amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                return;
            }

            if (amount > lastTransaction.Amount)
            {
                Console.WriteLine("Not enought funds");
                return;
            }

            var rate = currency.ToUpper() == "EUR" ? EURConversion : USDConversion;
            var quantity = amount / rate;
            Console.WriteLine($"Succesfully converted in {currency}: {quantity} {currency}");


            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = lastTransaction.Amount - amount,
                AmountUSD = currency == "USD"  ?lastTransaction.AmountUSD + quantity : lastTransaction.AmountUSD,
                AmountEUR = currency == "EUR" ? lastTransaction.AmountEUR + quantity : lastTransaction.AmountEUR,
                TransactionType = $"Conversion to {currency}"
            };

            newUser.TransactionHistories.Add(newTransaction);

            SaveNewUser(newUser);

            TriggerTransactionEvent($"User converted {amount} GEL to {currency}");
        }

        public static void TriggerTransactionEvent(string message, string type = "transaction")
        {
            OnTransaction?.Invoke(message, type);
        }

        public static void ListenToEvents()
        {
            var logger = new Logger();
            OnTransaction += logger.Log;
        }
    }
}
