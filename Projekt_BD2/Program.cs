// Plik: Program.cs
using System;
using System.IO;
using UdtReaderApp;
using UdtReaderApp.Services;

namespace UdtReaderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ////////////////// NALEŻY SIĘ UPEWNIĆ, ŻE PLIK connection.txt ISTNIEJE ORAZ //////////////////
            ////////////////// ZAWIERA POPRAWNY CONNECTION STRING DO BAZY DANYCH SQL SERVER //////////////
            //// W miejsce Data Source=<server_name>;Initial Catalog=<database_name> /////////////////////
            // należy wpisać odpowiednią nazwę serwera i bazy danych z umieszczonymi odpowiednimi tabelami //////
            ////////////////// ////////////////// ////////////////// ////////////////// //////////////////
            string connectionStringPath = "connection.txt";
            ////////////////// ////////////////// ////////////////// ////////////////// //////////////////
            ////////////////// ////////////////// ////////////////// ////////////////// //////////////////
            string connectionString;
            try
            {
                connectionString = File.ReadAllText(connectionStringPath).Trim();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas odczytu pliku z connection stringiem: {ex.Message}");
                Console.ResetColor();
                return;
            }
            

            UserService userService = new UserService(connectionString);
            EmailApp emailApp = new EmailApp(userService);

            VectorService vectorService = new VectorService(connectionString);
            VectorApp vectorApp = new VectorApp(vectorService);

            UnitService unitService = new UnitService(connectionString);
            UnitApp unitApp = new UnitApp(unitService);

            LocationService locationService = new LocationService(connectionString);
            LocationApp locationApp = new LocationApp(locationService);

            ColorService colorService = new ColorService(connectionString);
            ColorApp colorApp = new ColorApp(colorService);

            CurrencyService currencyService = new CurrencyService(connectionString);
            CurrencyApp currencyApp = new CurrencyApp(currencyService);

            while (true)
            {
                Console.WriteLine("\n=== MENU GŁÓWNE ===");
                Console.WriteLine("1. Sekcja Email");
                Console.WriteLine("2. Sekcja Vector");
                Console.WriteLine("3. Sekcja Unit");
                Console.WriteLine("4. Sekcja Lokalizacje");
                Console.WriteLine("5. Sekcja Kolory");
                Console.WriteLine("6. Sekcja Waluty");
                Console.WriteLine("0. Wyjdź z aplikacji");
                Console.Write("Wybierz sekcję: ");
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        RunSection(emailApp, connectionString);
                        break;
                    case "2":
                        RunSection(vectorApp, connectionString);
                        break;
                    case "3":
                        RunSection(unitApp, connectionString);
                        break;
                    case "4":
                        RunSection(locationApp, connectionString);
                        break;
                    case "5":
                        RunSection(colorApp, connectionString);
                        break;
                    case "6":
                        RunSection(currencyApp, connectionString);
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

        static void RunSection(object app, string connectionString)
        {
            while (true)
            {
                Console.WriteLine("\nAby wrócić do menu głównego, wpisz 'back' i naciśnij Enter w dowolnym momencie.");
                try
                {
                    if (app is EmailApp emailApp)
                    {
                        emailApp.Run(connectionString);
                        break;
                    }
                    else if (app is VectorApp vectorApp)
                    {
                        vectorApp.Run(connectionString);
                        break;
                    }
                    else if (app is UnitApp unitApp)
                    {
                        unitApp.Run(connectionString);
                        break;
                    }
                    else if (app is LocationApp locationApp)
                    {
                        locationApp.Run(connectionString);
                        break;
                    }
                    else if (app is ColorApp colorApp)
                    {
                        colorApp.Run(connectionString);
                        break;
                    }
                    else if (app is CurrencyApp currencyApp)
                    {
                        currencyApp.Run(connectionString);
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