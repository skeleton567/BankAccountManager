using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
     class Logger
    {
        public readonly string TransactionFilePath = "transaction_log.txt";

        public readonly string ErrorFilePath = "error_log.txt";

        public void Log(string message, string type = "transaction")
        {
            var logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";
            var logFilePath = type.ToLower() == "error" ? ErrorFilePath : TransactionFilePath;
            File.AppendAllText(logFilePath, logMessage);
        }

    }
}
