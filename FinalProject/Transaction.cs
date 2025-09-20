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
    static class Transaction
    {
        public static readonly decimal EURConversion = 2.85m;

        public static readonly decimal USDConversion = 2.6m;

        public static void CheckDeposit(User newUser)
        {
            var user = FindOrCreateUserJson(newUser);

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().Amount} GEl");

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountUSD} USD");

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountEUR} EUR");
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
            var newBalanceUSD = lastTransaction.AmountUSD - amount / USDConversion;
            var newBalanceEUR = lastTransaction.AmountEUR - amount / EURConversion;
            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = newBalance,
                AmountUSD = newBalanceUSD,
                AmountEUR = newBalanceEUR,
                TransactionType = "Withdraw"
            };
            user.TransactionHistories.Add(newTransaction);
            SaveNewUser(user);
            Console.WriteLine($"Successfully withdrew {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");
            Console.WriteLine($"New balance is: {newBalanceUSD} USD");
            Console.WriteLine($"New balance is: {newBalanceEUR} EUR");
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
            var newBalanceUSD = lastTransaction.AmountUSD + amount / USDConversion;
            var newBalanceEUR = lastTransaction.AmountEUR + amount / EURConversion;
            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = newBalance,
                AmountUSD = newBalanceUSD,
                AmountEUR = newBalanceEUR,
                TransactionType = "Deposit"
            };
            user.TransactionHistories.Add(newTransaction);
            SaveNewUser(user);
            Console.WriteLine($"Successfully deposited {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");
            Console.WriteLine($"New balance is: {newBalanceUSD} USD");
            Console.WriteLine($"New balance is: {newBalanceEUR} EUR");
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

                decimal gelChange;
                decimal usdChange;
                decimal eurChange;

                if (previousTransaction != null && (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Withdraw"))
                {
                    gelChange = previousTransaction.Amount - transaction.Amount;
                    usdChange = previousTransaction.AmountUSD - transaction.AmountUSD;
                    eurChange = previousTransaction.AmountEUR - transaction.AmountEUR;
                }
                else
                {
                    gelChange = transaction.Amount;
                    usdChange = transaction.AmountUSD;
                    eurChange = transaction.AmountEUR;
                }

                Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} of {Math.Abs(gelChange)} GEL ({Math.Abs(usdChange)} USD, {Math.Abs(eurChange)} EUR)");
            }
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
                    Console.Write("Enter amount in GEL to convert: ");
                    var inputUSD = Console.ReadLine() ?? string.Empty;
                    var amountUSD = decimal.TryParse(inputUSD, out decimal parsedAmount) ? parsedAmount : 0;
                    Console.WriteLine($"Here is your money in USD: {amountUSD / USDConversion} USD");
                    break;
                case "2":
                    Console.Write("Enter amount in GEL to convert: ");
                    var inputEUR = Console.ReadLine() ?? string.Empty;
                    var amountEUR = decimal.TryParse(inputEUR, out decimal parsedAmountEUR) ? parsedAmountEUR : 0;
                    Console.WriteLine($"Here is your money in USD: {amountEUR / EURConversion} EUR");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
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
                Console.WriteLine(e.Message);
                return new User();
            }
        }

        public static User SaveNewUser(User newUser)
        {
            var jsonString = JsonSerializer.Serialize(newUser);
            File.WriteAllText("user.json", jsonString);
            var user = JsonSerializer.Deserialize<User>(jsonString);

            return user!;
        }
    }
}
