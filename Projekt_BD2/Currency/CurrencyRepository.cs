using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UdtReaderApp.Models;

namespace UdtReaderApp.Repositories
{
    public class CurrencyRepository
    {
        private readonly string _connectionString;

        public CurrencyRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddCurrency(Currency currency)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Currency (MoneyValue) VALUES (@MoneyValue)";
                using (var command = new SqlCommand(query, connection))
                {
                    var param = new SqlParameter("@MoneyValue", currency.MoneyValue);
                    param.SqlDbType = SqlDbType.Udt;
                    param.UdtTypeName = "MoneyType";
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Currency> GetAllCurrencies()
        {
            var currencies = new List<Currency>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, MoneyValue FROM Currency";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currencies.Add(new Currency
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                MoneyValue = reader.IsDBNull(reader.GetOrdinal("MoneyValue"))
                                    ? MoneyType.Null
                                    : (MoneyType)reader.GetValue(reader.GetOrdinal("MoneyValue"))
                            });
                        }
                    }
                }
            }
            return currencies;
        }

        public Currency GetCurrencyById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, MoneyValue FROM Currency WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Currency
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                MoneyValue = reader.IsDBNull(reader.GetOrdinal("MoneyValue"))
                                    ? MoneyType.Null
                                    : (MoneyType)reader.GetValue(reader.GetOrdinal("MoneyValue"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void DeleteCurrency(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Currency WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}