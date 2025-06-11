using System;
using UdtReaderApp.Services;
using System.Data.SqlTypes; // Potrzebne dla SqlString w Email.Parse

namespace UdtReaderApp
{
    // Klasa EmailApp będzie zarządzać interakcją z użytkownikiem dla typu Email
    public class EmailApp
    {
        private readonly UserService _userService;

        // Konstruktor przyjmuje UserService, co pozwala na wstrzyknięcie zależności
        public EmailApp(UserService userService)
        {
            _userService = userService;
        }

        // Metoda Run zawiera całą logikę menu i interakcji dla Email
        public void Run()
        {
            Console.WriteLine("--- Aplikacja Zarządzania Użytkownikami (CLR UDT Email) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nowego użytkownika");
                Console.WriteLine("2. Wyświetl wszystkich użytkowników");
                Console.WriteLine("3. Wyszukaj użytkowników po części emaila (nazwa użytkownika/domena)");
                Console.WriteLine("4. Usuń użytkownika po ID");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            
                            Console.Write("Podaj adres e-mail (np. user@example.com): ");
                            string newEmail = Console.ReadLine();
                            _userService.AddUser(newEmail);
                            break;

                        case "2":
                            _userService.DisplayUsers(_userService.GetAllUsers());
                            break;

                        case "3":
                            Console.Write("Podaj fragment adresu e-mail do wyszukania: ");
                            string searchPart = Console.ReadLine();
                            _userService.DisplayUsers(_userService.SearchUsers("name", searchPart));
                            break;

                        case "4": // Opcja "Usuń użytkownika po ID" stała się case "4" po usunięciu poprzedniej "4"
                            Console.Write("Podaj ID użytkownika do usunięcia: ");
                            if (!int.TryParse(Console.ReadLine(), out int deleteId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            _userService.DeleteUser(deleteId);
                            break;

                        case "0":
                            Console.WriteLine("Zamykanie aplikacji...");
                            return; // Wyjdź z Run() i wróć do Main

                        default:
                            Console.WriteLine("Nieprawidłowa opcja. Spróbuj ponownie.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Wystąpił błąd: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }
}