using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using AwesomeGIC_Bank.Models;
using Rule = AwesomeGIC_Bank.Models.Rule;
using System.Transactions;
using Transaction = AwesomeGIC_Bank.Models.Transaction;

namespace AwesomeGIC_Bank.services
{
    public class BankingService
    {
        private string _connectionString;
        public BankingService()
        {
            _connectionString = "Server=localhost;Database=AwesomeGIC;Trusted_Connection=True;MultipleActiveResultSets=true";
        }
        //public BankingService(IConfiguration iconfiguration)
        //{
        //    _connectionString = iconfiguration.GetConnectionString("DefaultConnection");
        //}
        public bool ValidateDate(string dateDetails)
        {
            bool checkNumber = int.TryParse(dateDetails, out int TransactionDate);
            int month = Convert.ToInt32(dateDetails.Substring(4, 2));
            int year = Convert.ToInt32(dateDetails.Substring(0, 4));
            int day = Convert.ToInt32(dateDetails.Substring(6, 2));

            if (dateDetails.Length == 8 && checkNumber && month > 0 && month < 13 && day > 0 && day <= DateTime.DaysInMonth(year, month) && year >= 1900)
            {
                return true;
            }
            else
                return false;
        }

        public bool ValidateName(string name)
        {
            if (name.Length > 0 && name.Length < 51)
            {
                return true;
            }
            else
                return false;
        }

        public bool ValidateAmount(string amount)
        {
            bool checkAmount = decimal.TryParse(amount, out decimal Amount);
            if (checkAmount && Amount > 0)
            {
                return true;
            }
            else return false;
        }

        public bool ValidateInterestRate(string rate)
        {
            bool checkRate = decimal.TryParse(rate, out decimal Rate);
            if (checkRate && Rate > 0 && Rate < 100)
            {
                return true;
            }
            else return false;
        }

        public AccountDetails GetAccount(string Account)
        {
            AccountDetails Details = new AccountDetails();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("usp_CheckAccountBalance", connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddWithValue("@ACCOUNT", Account);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        //Convert the result into workflow model..
                        while (reader.Read())
                        {
                            Details.Account = reader["Account"].ToString();
                            Details.IsActive = Convert.ToBoolean(reader["IsActive"]);
                            Details.Balance = Convert.ToDecimal(reader["Balance"]);
                        }

                    }
                }
            }
            return Details;
        }

        public List<Transaction> AddTransaction(string Date, string Account, string Type, string Amount)
        {
            List<Transaction> Transactions = new List<Transaction>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("usp_AddNewTransaction", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        command.Parameters.AddWithValue("@DATE", Date);
                        command.Parameters.AddWithValue("@ACCOUNT", Account);
                        command.Parameters.AddWithValue("@TYPE", Type);
                        command.Parameters.AddWithValue("@AMOUNT", Convert.ToDecimal(Amount));
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            //Convert the result into workflow model..
                            while (reader.Read())
                            {
                                Transactions.Add(new Transaction()
                                {
                                    Date = reader["DATE"].ToString(),
                                    Account = reader["ACCOUNT"].ToString(),
                                    TransactionId = reader["TRANSACTIONID"].ToString(),
                                    Type = reader["TYPE"].ToString(),
                                    Amount = Convert.ToDecimal(reader["AMOUNT"]),
                                    Balance = Convert.ToDecimal(reader["BALANCE"])
                                });
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
            return Transactions;
        }

        public List<Rule> AddRule(string Date, string Rule, string Rate)
        {
            List<Rule> Rules = new List<Rule>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("usp_AddNewRule", connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddWithValue("@DATE", Date);
                    command.Parameters.AddWithValue("@RULEID", Rule);
                    command.Parameters.AddWithValue("@RATE", Convert.ToDecimal(Rate));

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        //Convert the result into workflow model..
                        while (reader.Read())
                        {
                            Rules.Add(new Rule()
                            {
                                Date = reader["DATE"].ToString(),
                                RuleId = reader["RULEID"].ToString(),
                                Rate = Convert.ToDecimal(reader["RATE"])
                            });
                        }

                    }
                }
            }
            return Rules;
        }

        public List<Transaction> GetTransactions(string Account, string Month)
        {
            List<Transaction> Transactions = new List<Transaction>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetStatement", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        command.Parameters.AddWithValue("@ACCOUNT", Account);
                        command.Parameters.AddWithValue("@MONTH", Convert.ToInt32(Month));

                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            //Convert the result into workflow model..
                            while (reader.Read())
                            {
                                Transactions.Add(new Transaction()
                                {
                                    Date = reader["DATE"].ToString(),
                                    Account = reader["ACCOUNT"].ToString(),
                                    TransactionId = reader["TRANSACTIONID"].ToString(),
                                    Type = reader["TYPE"].ToString(),
                                    Amount = Convert.ToDecimal(reader["AMOUNT"]),
                                    Balance = Convert.ToDecimal(reader["BALANCE"])
                                });
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return Transactions;
        }

        public List<Rule> GetRules(string Month)
        {
            List<Rule> Rules = new List<Rule>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("usp_GetRules", connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddWithValue("@MONTH", Month);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        //Convert the result into workflow model..
                        while (reader.Read())
                        {
                            Rules.Add(new Rule()
                            {
                                Date = reader["DATE"].ToString(),
                                RuleId = reader["RULEID"].ToString(),
                                Rate = Convert.ToDecimal(reader["RATE"])
                            });
                        }

                    }
                }
            }
            return Rules;
        }
    }
}
