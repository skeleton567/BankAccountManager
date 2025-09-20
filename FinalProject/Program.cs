
using FinalProject;

Console.WriteLine("Enter Card details:");
    Console.Write("Card Number: ");
    var cardNumber = Console.ReadLine() ?? string.Empty;

    if (cardNumber.Length != 1 || !long.TryParse(cardNumber, out _))
    {
        Console.WriteLine("Invalid card number. It must be 16 digits.");
        return;
    }

Console.Write("CVV: ");
    var cvv = Console.ReadLine() ?? string.Empty;

    if (cvv.Length != 3 || !int.TryParse(cvv, out _))
    {
        Console.WriteLine("Invalid CVV. It must be 3 digits.");
        return;
    }

Console.Write("Expiration Date (MM/YY): ");
    var expirationDate = Console.ReadLine() ?? string.Empty;

    if (!DateTime.TryParseExact(expirationDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out DateTime expDate) || expDate < DateTime.Now)
    {
        Console.WriteLine("Invalid expiration date. Ensure it's in MM/YY format and not expired.");
        return;
    }

var newUser = new User() { CardNumber = cardNumber, Cvv = cvv, ExpirationDate = expDate };

Transaction.SaveNewUser(newUser);

while (true)
{
    var user = Transaction.FindOrCreateUserJson(newUser);
    Console.WriteLine("Please enter your 4-digit PIN to log in:");
    Console.Write("PIN: ");
    var pin = Console.ReadLine() ?? string.Empty;

    // Initial PIN is "1234"
    if (pin.Length != 4 || !int.TryParse(pin, out _) || user.Pin != pin)
    {
        Console.WriteLine("Invalid PIN. It must be 4 digits.");
        continue;
    }

    newUser.Pin = pin;

    Transaction.FindOrCreateUserJson(newUser);

    Console.WriteLine("You loged in succesfully. Please choose and action:");

    Console.WriteLine("1. Check Balance");
    Console.WriteLine("2. Withdraw");
    Console.WriteLine("3. Deposit");
    Console.WriteLine("4. Get Last 5 transactions");
    Console.WriteLine("5. Change pin");
    Console.WriteLine("6. Money conversion");
    Console.Write("Select an option (1-6): ");

    var choice = Console.ReadLine() ?? string.Empty;

    switch (choice)
    {
        case "1":
            Transaction.CheckDeposit(newUser);
            break;
        case "2":
            Transaction.Withdraw(newUser);
            break;
        case "3":
            Console.Write("Enter amount to deposit: ");
            var amount = Console.ReadLine() ?? string.Empty;
            Transaction.Deposit(newUser, amount);
            break;
        case "4":
           Transaction.getLastFiveTransactions(newUser);
            break;
        case "5":
           Transaction.ChangePin(newUser);
            break;
        case "6":
           Transaction.MoneyConversion(newUser);
            break;
        default:
            Console.WriteLine("Invalid option. Please select a number between 1 and 6.");
            break;
     }
}