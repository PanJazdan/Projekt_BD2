// Plik: Program.cs
using System;
using UdtReaderApp.Services; // Potrzebne do stworzenia UserService

namespace UdtReaderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=LAPTOPJAN\SQLEXPRESS;
                                           Initial Catalog=BD_Projekt;
                                           Integrated Security=True;
                                           Encrypt=False";

            UserService userService = new UserService(connectionString);
            EmailApp emailApp = new EmailApp(userService);

            VectorService vectorService = new VectorService(connectionString);
            VectorApp vectorApp = new VectorApp(vectorService);

            while (true)
            {
                Console.WriteLine("\n=== MENU GŁÓWNE ===");
                Console.WriteLine("1. Sekcja Email");
                Console.WriteLine("2. Sekcja Vector");
                Console.WriteLine("0. Wyjdź z aplikacji");
                Console.Write("Wybierz sekcję: ");
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        RunSection(emailApp);
                        break;
                    case "2":
                        RunSection(vectorApp);
                        break;
                    case "0":
                        Console.WriteLine("Zamykanie aplikacji...");
                        return;
                    default:
                        Console.WriteLine("Nieprawidłowa opcja. Spróbuj ponownie.");
                        break;
                }
            }
        }

        // Metoda uruchamiająca sekcję z możliwością powrotu do menu głównego
        static void RunSection(object app)
        {
            while (true)
            {
                Console.WriteLine("\nAby wrócić do menu głównego, wpisz 'back' i naciśnij Enter w dowolnym momencie.");
                try
                {
                    if (app is EmailApp emailApp)
                    {
                        emailApp.Run();
                        break;
                    }
                    else if (app is VectorApp vectorApp)
                    {
                        vectorApp.Run();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Wystąpił błąd: {ex.Message}");
                    Console.ResetColor();
                }
                break;
            }
        }
    }
}