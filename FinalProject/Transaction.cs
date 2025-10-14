using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject
{
    public delegate Task TransactionDelegate(string message, string type = "transaction");

    static class Transaction
    {
        public static readonly decimal EURConversion = 2.85m;
        public static readonly decimal USDConversion = 2.6m;

        public static event TransactionDelegate? OnTransaction;

        public static async Task CheckDepositAsync(User newUser)
        {
            var user = await FindOrCreateUserJsonAsync(newUser);

            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().Amount} GEL");
            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountUSD} USD");
            Console.WriteLine($"Your current balance is: {user.TransactionHistories.Last().AmountEUR} EUR");

            await TriggerTransactionEventAsync("User checked their balance.");
        }

        public static async Task WithdrawAsync(User newUser)
        {
            var user = await FindOrCreateUserJsonAsync(newUser);
            var lastTransaction = user.TransactionHistories.Last();

            Console.Write("Enter amount to withdraw: ");
            var input = Console.ReadLine() ?? string.Empty;
            var amount = decimal.TryParse(input, out var parsedAmount) ? parsedAmount : 0;

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
            await SaveNewUserAsync(user);

            Console.WriteLine($"Successfully withdrew {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");

            await TriggerTransactionEventAsync($"User withdrew {amount}. New balance is: {newBalance} GEL");
        }

        public static async Task DepositAsync(User newUser)
        {
            Console.Write("Enter amount to deposit: ");
            var amountInput = Console.ReadLine() ?? string.Empty;

            var amount = decimal.TryParse(amountInput, out var parsedAmount) ? parsedAmount : 0;
            if (amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                return;
            }

            var user = await FindOrCreateUserJsonAsync(newUser);
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
            await SaveNewUserAsync(user);

            Console.WriteLine($"Successfully deposited {amount} GEL.");
            Console.WriteLine($"New balance is: {newBalance} GEL");

            await TriggerTransactionEventAsync($"User deposited {amount}. New balance is: {newBalance} GEL");
        }

        public static async Task GetLastFiveTransactionsAsync(User newUser)
        {
            var user = await FindOrCreateUserJsonAsync(newUser);
            var transactions = user.TransactionHistories
                .OrderByDescending(t => t.TransactionDate)
                .Take(6)
                .ToList();

            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine("Last 5 transactions:");

            for (int i = 0; i < transactions.Count - 1; i++)
            {
                var transaction = transactions[i];
                var previous = transactions.ElementAtOrDefault(i + 1);

                if (transaction.TransactionType.Contains("Conversion") && previous != null)
                {
                    var gelChange = previous.Amount - transaction.Amount;
                    var currency = transaction.TransactionType.Contains("USD") ? "USD" : "EUR";
                    var change = currency == "USD"
                        ? transaction.AmountUSD - previous.AmountUSD
                        : transaction.AmountEUR - previous.AmountEUR;

                    Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} {gelChange} GEL converted to {change} {currency}");
                }
                else
                {
                    var gelChange = previous != null
                        ? previous.Amount - transaction.Amount
                        : transaction.Amount;

                    Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} of {Math.Abs(gelChange)} GEL");
                }
            }

            await TriggerTransactionEventAsync("User checked last 5 transactions.");
        }

        public static async Task ChangePinAsync(User newUser)
        {
            var user = await FindOrCreateUserJsonAsync(newUser);

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
            await SaveNewUserAsync(user);

            Console.WriteLine("PIN successfully changed.");
            await TriggerTransactionEventAsync("User changed PIN.");
        }

        public static async Task MoneyConversionAsync(User newUser)
        {
            Console.WriteLine("Choose currency to convert to:");
            Console.WriteLine("1. USD");
            Console.WriteLine("2. EUR");
            Console.Write("Select an option (1-2): ");
            var choice = Console.ReadLine() ?? string.Empty;

            switch (choice)
            {
                case "1":
                    await ConvertAmountAsync(newUser, "USD");
                    break;
                case "2":
                    await ConvertAmountAsync(newUser, "EUR");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            await TriggerTransactionEventAsync("User converted money.");
        }

        public static async Task<User> FindOrCreateUserJsonAsync(User newUser)
        {
            try
            {
                if (File.Exists("user.json"))
                {
                    var jsonString = await File.ReadAllTextAsync("user.json");
                    var user = JsonSerializer.Deserialize<User>(jsonString);
                    if (user != null && user.CardNumber == newUser.CardNumber)
                        return user;
                }

                return await SaveNewUserAsync(newUser);
            }
            catch (Exception ex)
            {
                await TriggerTransactionEventAsync(ex.Message, "error");
                return await SaveNewUserAsync(newUser);
            }
        }

        public static async Task<User> SaveNewUserAsync(User newUser)
        {
            var jsonString = JsonSerializer.Serialize(newUser, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync("user.json", jsonString);
            return newUser;
        }

        private static async Task ConvertAmountAsync(User newUser, string currency = "USD")
        {
            Console.Write("Enter amount in GEL to convert: ");
            var input = Console.ReadLine() ?? string.Empty;
            var amount = decimal.TryParse(input, out var parsedAmount) ? parsedAmount : 0;

            var lastTransaction = newUser.TransactionHistories.Last();

            if (amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                return;
            }
            if (amount > lastTransaction.Amount)
            {
                Console.WriteLine("Not enough funds.");
                return;
            }

            var rate = currency.ToUpper() == "EUR" ? EURConversion : USDConversion;
            var converted = amount / rate;

            Console.WriteLine($"Successfully converted to {currency}: {converted} {currency}");

            var newTransaction = new TransactionHistory
            {
                TransactionDate = DateTime.Now,
                Amount = lastTransaction.Amount - amount,
                AmountUSD = currency == "USD" ? lastTransaction.AmountUSD + converted : lastTransaction.AmountUSD,
                AmountEUR = currency == "EUR" ? lastTransaction.AmountEUR + converted : lastTransaction.AmountEUR,
                TransactionType = $"Conversion to {currency}"
            };

            newUser.TransactionHistories.Add(newTransaction);
            await SaveNewUserAsync(newUser);
            await TriggerTransactionEventAsync($"User converted {amount} GEL to {currency}");
        }

        public static async Task TriggerTransactionEventAsync(string message, string type = "transaction")
        {
            if (OnTransaction != null)
            {
                var invocationList = OnTransaction.GetInvocationList();
                foreach (TransactionDelegate handler in invocationList)
                {
                    await handler.Invoke(message, type);
                }
            }
        }

        public static void ListenToEvents()
        {
            var logger = new Logger();
            OnTransaction += logger.LogAsync;
        }
    }
}
