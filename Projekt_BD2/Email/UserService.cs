using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);
        }

        public bool AddUser(string emailString)
        {
            try
            {
                Email email = Email.Parse(new System.Data.SqlTypes.SqlString(emailString)); // Używamy Parse z UDT
                var user = new User {ContactEmail = email };
                _userRepository.AddUser(user);
                Console.WriteLine($"Dodano użytkownika: Email={emailString}");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd walidacji adresu e-mail: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania użytkownika: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            Console.WriteLine("\n--- Wszyscy użytkownicy ---");
            return _userRepository.GetAllUsers();
        }

        public List<User> SearchUsers(string type, string searchTerm)
        {
            Console.WriteLine($"\n--- Wyszukiwanie użytkowników po {type} ({searchTerm}) ---");
            if (type.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                // W tym przypadku 'name' oznacza ogólne wyszukiwanie w adresie email
                // bo nie mamy kolumny 'Name' w tabeli Users
                return _userRepository.SearchUsersByEmailPart(searchTerm);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Nieznany typ wyszukiwania. Użyj 'name' lub 'domain'.");
                Console.ResetColor();
                return new List<User>();
            }
        }

        public void DeleteUser(int id)
        {
            Console.WriteLine($"\n--- Usuwanie użytkownika o ID: {id} ---");
            _userRepository.DeleteUser(id);
        }

        public void DisplayUsers(List<User> users)
        {
            if (users == null || users.Count == 0)
            {
                Console.WriteLine("Brak użytkowników do wyświetlenia.");
                return;
            }

            // Print header with fixed widths
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-30}", "ID", "Username", "Domena", "Email");
            Console.WriteLine("------------------------------------------------------------------");

            foreach (var user in users)
            {
                if (!user.ContactEmail.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-30}",
                        user.Id,
                        user.ContactEmail.username,
                        user.ContactEmail.domain,
                        user.ContactEmail.ToString());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-30}",
                        user.Id, "NULL", "NULL", "NULL");
                }
            }
            Console.WriteLine("------------------------------------------------------------------");
        }
    }
}