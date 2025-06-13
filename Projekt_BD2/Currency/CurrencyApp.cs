using System;
using System.Collections.Generic;
using System.Reflection;
using UdtReaderApp.Models;
using UdtReaderApp.Services;
using UdtReaderApp.Utils;

namespace UdtReaderApp
{
    internal class CurrencyApp
    {
        private readonly CurrencyService _currencyService;

        public CurrencyApp(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        private void DisplaySupportedCurrencies()
        {
            // Use reflection to get the IsoCurrencies list from MoneyType
            var field = typeof(MoneyType).GetField("IsoCurrencies", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                var currencies = field.GetValue(null) as IEnumerable<string>;
                if (currencies != null)
                {
                    Console.WriteLine("Obsługiwane kody walut: " + string.Join(", ", currencies));
                }
            }
        }

        public void Run(string connectionString)
        {
            Console.WriteLine("--- Aplikacja Zarządzania Wartościami Pieniężnymi (CLR UDT Money) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nową wartość pieniężną");
                Console.WriteLine("2. Wyświetl wszystkie wartości");
                Console.WriteLine("3. Usuń wartość po ID");
                Console.WriteLine("4. Przelicz walutę na inną");
                Console.WriteLine("5. Dodaj rekordy przez plik CSV");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            DisplaySupportedCurrencies();
                            Console.Write("Podaj wartość w formacie 'kwota [KOD]' (np. 123.45 [PLN]): ");
                            string moneyString = Console.ReadLine();
                            _currencyService.AddCurrency(moneyString);
                            break;

                        case "2":
                            _currencyService.DisplayCurrencies(_currencyService.GetAllCurrencies());
                            break;

                        case "3":
                            Console.Write("Podaj ID wartości do usunięcia: ");
                            if (!int.TryParse(Console.ReadLine(), out int deleteId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            _currencyService.DeleteCurrency(deleteId);
                            break;

                        case "4":
                            Console.Write("Podaj ID wartości do przeliczenia: ");
                            if (!int.TryParse(Console.ReadLine(), out int convId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            DisplaySupportedCurrencies();
                            Console.Write("Podaj kod waluty: ");
                            string targetCurrency = Console.ReadLine();
                            try
                            {
                                var converted = _currencyService.ConvertCurrency(convId, targetCurrency);
                                Console.WriteLine($"Przeliczona wartość: {converted.ToString()}");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Błąd: " + ex.Message);
                                Console.ResetColor();
                            }
                            break;
                        case "5":
                            Console.Write("Podaj ścieżkę do pliku CSV: ");
                            string csvPath = Console.ReadLine();
                            CsvImporter.ImportCsvToTable(
                                connectionString,
                                "Currency",
                                csvPath,
                                new Dictionary<string, string> { { "MoneyValue", "MoneyType" } }
                            );
                            break;

                        case "0":
                            Console.WriteLine("Wyjście z aplikacji Wartości Pieniężnych.");
                            return;

                        default:
                            Console.WriteLine("Nieprawidłowa opcja.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Błąd: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}