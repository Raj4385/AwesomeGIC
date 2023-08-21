// See https://aka.ms/new-console-template for more information
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;
using AwesomeGIC_Bank.Models;
using AwesomeGIC_Bank.services;
using Rule = AwesomeGIC_Bank.Models.Rule;
using Transaction = AwesomeGIC_Bank.Models.Transaction;

bool finshed = false;
BankingService BankService = new BankingService();
do
{
    Console.Clear();

    Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
    Console.WriteLine("[I]nput transactions");
    Console.WriteLine("[D]efine interest rules");
    Console.WriteLine("[P]rint statement");
    Console.WriteLine("[Q]uit");
    string userInput = Console.ReadLine();

    Console.Clear();

    if (userInput.Equals("Q") || userInput.Equals("q"))
    {
        Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
        Console.WriteLine("Have a nice day!");
        finshed = true;
    }
    else if (userInput.Equals("I") || userInput.Equals("i"))
    {
        Console.WriteLine("Please enter transaction details in <Date>|<Account>|<Type>|<Amount> format (or enter blank to go back to main menu):");
        string transaction = Console.ReadLine();
        if (!string.IsNullOrEmpty(transaction))
        {
            string[] details = transaction.Split('|');
            if (details.Count() == 4)
            {
                if (BankService.ValidateDate(details[0]))
                {
                    if (BankService.ValidateName(details[1]))
                    {
                        if (details[2].Length == 1 && (details[2] == "W" || details[2] == "D" || details[2] == "w" || details[2] == "d"))
                        {
                            if (BankService.ValidateAmount(details[3]))
                            {
                                if (details[2] == "d" || details[2] == "D")
                                {
                                    List<Transaction> Transactions = BankService.AddTransaction(details[0], details[1], details[2], details[3]);
                                    Console.WriteLine("Account: " + details[1]);
                                    Console.WriteLine("Date     | Txn Id      | Type | Amount |");

                                    foreach (Transaction T in Transactions)
                                    {
                                        Console.WriteLine(T.Date + " | " + T.TransactionId + " | " + T.Type + " | " + T.Amount + " |");
                                    }

                                    Console.ReadLine();
                                }
                                else
                                {
                                    AccountDetails AccountDetail = BankService.GetAccount(details[1]);
                                    if (AccountDetail.IsActive && ((AccountDetail.Balance - Convert.ToDecimal(details[3])) > 0))
                                    {
                                        List<Transaction> Transactions = BankService.AddTransaction(details[0], details[1], details[2], details[3]);
                                        Console.WriteLine("Account: " + details[1]);
                                        Console.WriteLine("Date     | Txn Id      | Type | Amount |");

                                        foreach (Transaction T in Transactions)
                                        {
                                            Console.WriteLine(T.Date + " | " + T.TransactionId + " | " + T.Type + " | " + T.Amount + " |");
                                        }

                                        Console.ReadLine();
                                    }
                                    else
                                    {
                                        if (AccountDetail.IsActive)
                                            Console.WriteLine("Insufficent Balance...");
                                        else
                                            Console.WriteLine("Account not present...");

                                        Console.ReadLine();
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid amount value...");
                                Console.ReadLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid transaction type...");
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Account Number format...");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Date format...");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid input format. Please follow the template...");
                Console.ReadLine();
            }
        }
    }
    else if (userInput.Equals("D") || userInput.Equals("d"))
    {
        Console.WriteLine("Please enter interest rules details in <Date>|<RuleId>|<Rate in %> format(or enter blank to go back to main menu)");
        string rule = Console.ReadLine();
        if (!string.IsNullOrEmpty(rule))
        {
            string[] details = rule.Split('|');
            if (details.Count() == 3)
            {
                if (BankService.ValidateDate(details[0]))
                {
                    if (BankService.ValidateName(details[1]))
                    {
                        if (BankService.ValidateInterestRate(details[2]))
                        {
                            List<Rule> Rules = BankService.AddRule(details[0], details[1], details[2]);
                            Console.WriteLine("Date     | RuleId      | Rate (%) |");
                            foreach(Rule R in Rules)
                            {
                                Console.WriteLine(R.Date + " | " + R.RuleId + " | " + R.Rate + " |");
                            }
                            Console.ReadLine();
                        }
                    }
                }
            }
        }
    }
    else if (userInput.Equals("P") || userInput.Equals("p"))
    {
        Console.WriteLine("Please enter account and month to generate the statement <Account>|<Month> (or enter blank to go back to main menu)");
        string statement = Console.ReadLine();
        if (!string.IsNullOrEmpty(statement))
        {
            string[] details = statement.Split('|');
            if (details.Count() == 2)
            {
                bool checkNumber = int.TryParse(details[1], out int month);
                if (BankService.ValidateName(details[0]))
                {
                    if (checkNumber && month > 0 && month < 13)
                    {
                        AccountDetails AccountDetail = BankService.GetAccount(details[0]);
                        List<Transaction> Transactions = BankService.GetTransactions(details[0], details[1]);
                        List<Rule> Rules = BankService.GetRules(details[1]);

                        decimal Interest = 0;
                        int currentMonth = DateTime.Now.Month;
                        int currentDay = DateTime.Now.Day;
                        int currentYear = DateTime.Now.Year;
                        int totalMonthDays = DateTime.DaysInMonth(currentYear, currentMonth);
                        int MonthDays = DateTime.DaysInMonth(currentYear, month);

                        if (((month < currentMonth) || (month == currentMonth && currentDay == totalMonthDays)) && Rules.Count() > 0 && Transactions.Count() > 0)
                        {
                            int Count = 1;
                            //decimal Interest = 0;
                            decimal Rate = 0;
                            decimal Balance = 0;
                            List<RuleCalculation> calculations = new List<RuleCalculation>();
                            DateTime StartDate = new DateTime(currentYear, month, 1);
                            DateTime EndDate = new DateTime(currentYear, month, MonthDays);
                            DateTime MaxDate = Transactions.Max(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2))));
                            Rule FirstRule = Rules.OrderByDescending(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2)))).FirstOrDefault(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2))) < StartDate);
                            Transaction FirstTran = Transactions.OrderBy(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2)))).FirstOrDefault();
                            if (FirstTran != null)
                            {
                                if (FirstTran.Type == "W")
                                    Balance = FirstTran.Balance + FirstTran.Amount;
                                else
                                    Balance = FirstTran.Balance - FirstTran.Amount;
                            }
                            if (FirstRule != null)
                            {
                                Rate = FirstRule.Rate;
                            }

                            for (DateTime i = StartDate; i <= EndDate; i = i.AddDays(1))
                            {
                                Rule CurrentRule = Rules.OrderByDescending(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2)))).FirstOrDefault(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2))) == i);
                                List<Transaction> CurrentTransactions = Transactions.Where(x => new DateTime(Convert.ToInt32(x.Date.Substring(0, 4)), Convert.ToInt32(x.Date.Substring(4, 2)), Convert.ToInt32(x.Date.Substring(6, 2))) == i).ToList();
                                Transaction CurrentTransaction = null;

                                if (CurrentTransactions != null && CurrentTransactions.Count() > 0)
                                {
                                    CurrentTransaction = CurrentTransactions.OrderByDescending(x => Convert.ToInt32(x.TransactionId.Substring(9))).FirstOrDefault();
                                }

                                if (CurrentRule != null || CurrentTransaction != null)
                                {
                                    if (Count != 1)
                                    {
                                        //if (EndDate == i)
                                        Interest += (Count * Rate * Balance) / 100;
                                        //else
                                        //    Interest += (Count - 1) * Rate * Balance;
                                    }

                                    Count = 0;
                                    Rate = (CurrentRule != null) ? CurrentRule.Rate : Rate;
                                    Balance = (CurrentTransaction != null) ? CurrentTransaction.Balance : Balance;
                                }
                                Count++;
                                if (EndDate == i)
                                {
                                    Interest += (Count * Rate * Balance) / 100;
                                }
                            }
                        }
                        Interest = (Interest == 0) ? 0 : Math.Round((Interest / 365), 2);
                        AwesomeGIC_Bank.Models.Transaction Transaction = new AwesomeGIC_Bank.Models.Transaction();
                        Transaction.Account = details[0];
                        Transaction.Amount = Interest;
                        Transaction.Balance = AccountDetail.Balance + Interest;
                        Transaction.Date = DateTime.Now.Year.ToString() + details[1] + MonthDays;
                        Transaction.TransactionId = String.Empty;
                        Transaction.Type = "I";

                        Transactions.Add(Transaction);

                        Console.WriteLine("Account: " + details[0]);
                        Console.WriteLine("Date | Txn Id | Type | Amount | Balance |");
                        foreach (Transaction FT in Transactions)
                        {
                            Console.WriteLine(FT.Date + " | " + FT.TransactionId + " | " + FT.Type + " | " + FT.Amount + " | " + FT.Balance + " |");
                        }
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Invalid month field...");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Account Number format...");
                    Console.ReadLine();
                }
            }
        }
    }

}while (!finshed);

