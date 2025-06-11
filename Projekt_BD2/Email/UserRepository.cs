using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UdtReaderApp.Models;

namespace UdtReaderApp.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (ContactEmail) VALUES (@ContactEmail)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Usunięto parametr @Id, bo Id jest generowane automatycznie przez IDENTITY

                    SqlParameter emailParam = new SqlParameter("@ContactEmail", user.ContactEmail);
                    emailParam.SqlDbType = SqlDbType.Udt;
                    emailParam.UdtTypeName = "Email";

                    command.Parameters.Add(emailParam);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ContactEmail FROM Users";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id"))
                            };

                            if (reader.IsDBNull(reader.GetOrdinal("ContactEmail")))
                            {
                                user.ContactEmail = Email.Null;
                            }
                            else
                            {
                                user.ContactEmail = (Email)reader.GetValue(reader.GetOrdinal("ContactEmail"));
                            }
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        public List<User> SearchUsersByEmailPart(string searchPart)
        {
            List<User> users = new List<User>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ContactEmail FROM Users WHERE ContactEmail.ToString() LIKE @SearchPart";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchPart", $"%{searchPart}%");
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id"))
                            };
                            if (reader.IsDBNull(reader.GetOrdinal("ContactEmail")))
                            {
                                user.ContactEmail = Email.Null;
                            }
                            else
                            {
                                user.ContactEmail = (Email)reader.GetValue(reader.GetOrdinal("ContactEmail"));
                            }
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        public void DeleteUser(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Users WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"Użytkownik o ID: {id} nie został znaleziony.");
                    }
                    else
                    {
                        Console.WriteLine($"Użytkownik o ID: {id} został pomyślnie usunięty.");
                    }
                }
            }
        }
    }
}