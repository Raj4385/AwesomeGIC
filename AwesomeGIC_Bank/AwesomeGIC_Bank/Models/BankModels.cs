using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGIC_Bank.Models
{
    internal class BankModels
    {
    }

    public class AccountDetails
    {
        public string Account { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }
    }

    public class Transaction
    {
        public string Date { get; set; }
        public string Account { get; set; }
        public string TransactionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }

    public class Rule
    {
        public string Date { get; set; }
        public string RuleId { get; set; }
        public decimal Rate { get; set; }
    }

    public class RuleCalculation
    {
        public int Days { get; set; }
        public decimal Rate { get; set; }
        public decimal Balance { get; set; }
    }
}
