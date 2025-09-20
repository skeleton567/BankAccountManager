using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
     class Logger
    {
        public readonly string LogFilePath = "transaction_log.txt";

        public void Log(string message)
        {
            var logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, logMessage);
        }
    }
}
