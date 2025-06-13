using System;
using System.Collections.Generic;
using System.Globalization;
using UdtReaderApp.Models;
using UdtReaderApp.Services;
using UdtReaderApp.Utils;

namespace UdtReaderApp
{
    // Klasa UnitApp zarządza interakcją z użytkownikiem dla typu UnitSI
    internal class UnitApp
    {
        private readonly UnitService _unitService;

        // Konstruktor przyjmuje UnitService (wstrzyknięcie zależności)
        public UnitApp(UnitService unitService)
        {
            _unitService = unitService;
        }

        // Metoda Run zawiera całą logikę menu i interakcji dla UnitSI
        public void Run(string connectionString)
        {
            Console.WriteLine("--- Aplikacja Zarządzania Jednostkami (CLR UDT UnitSI) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nową jednostkę");
                Console.WriteLine("2. Wyświetl wszystkie jednostki");
                Console.WriteLine("3. Dodaj do siebie dwie jednostki (podaj ID)");
                Console.WriteLine("4. Odejmij od siebie dwie jednostki (podaj ID)");
                Console.WriteLine("5. Pomnóż dwie jednostki (podaj ID)");
                Console.WriteLine("6. Podziel dwie jednostki (podaj ID)");
                Console.WriteLine("7. Pomnóż jednostkę przez skalar");
                Console.WriteLine("8. Podziel jednostkę przez skalar");
                Console.WriteLine("9. Wyświetl jednostkę z przedrostkiem");
                Console.WriteLine("10. Dodaj rekordy przez plik CSV");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.Write("Podaj jednostkę w formacie 'wartość [jednostka]' (np. 5.0 [kg]): ");
                            string newUnit = Console.ReadLine();
                            _unitService.AddUnit(newUnit);
                            break;

                        case "2":
                            _unitService.DisplayUnits(_unitService.GetAllUnits());
                            break;

                        case "3":
                            Console.Write("Podaj ID pierwszej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int addId1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int addId2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var addResult = _unitService.AddUnits(addId1, addId2);
                            Console.WriteLine("Wynik dodawania: " + addResult.ToString());
                            break;

                        case "4":
                            Console.Write("Podaj ID pierwszej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int subId1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int subId2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var subResult = _unitService.SubtractUnits(subId1, subId2);
                            Console.WriteLine("Wynik odejmowania: " + subResult.ToString());
                            break;

                        case "5":
                            Console.Write("Podaj ID pierwszej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int mulId1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int mulId2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var mulResult = _unitService.MultiplyUnits(mulId1, mulId2);
                            Console.WriteLine("Wynik mnożenia: " + mulResult.ToString());
                            break;

                        case "6":
                            Console.Write("Podaj ID pierwszej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int divId1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiej jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int divId2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var divResult = _unitService.DivideUnits(divId1, divId2);
                            Console.WriteLine("Wynik dzielenia: " + divResult.ToString());
                            break;

                        case "7":
                            Console.Write("Podaj ID jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int mulSId))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj skalar: ");
                            if (!double.TryParse(Console.ReadLine(), NumberStyles.Float, CultureInfo.InvariantCulture, out double scalarMul))
                            {
                                Console.WriteLine("Nieprawidłowy skalar.");
                                break;
                            }
                            var mulSResult = _unitService.MultiplyUnitByScalar(mulSId, scalarMul);
                            Console.WriteLine("Wynik mnożenia przez skalar: " + mulSResult.ToString());
                            break;

                        case "8":
                            Console.Write("Podaj ID jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int divSId))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj skalar: ");
                            if (!double.TryParse(Console.ReadLine(), NumberStyles.Float, CultureInfo.InvariantCulture, out double scalarDiv))
                            {
                                Console.WriteLine("Nieprawidłowy skalar.");
                                break;
                            }
                            var divSResult = _unitService.DivideUnitByScalar(divSId, scalarDiv);
                            Console.WriteLine("Wynik dzielenia przez skalar: " + divSResult.ToString());
                            break;

                        case "9":
                            Console.Write("Podaj ID jednostki: ");
                            if (!int.TryParse(Console.ReadLine(), out int prefixId))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj przedrostek (np. k, m, c, G): ");
                            string prefix = Console.ReadLine();
                            var prefixed = _unitService.GetUnitWithPrefix(prefixId, prefix);
                            Console.WriteLine("Jednostka z przedrostkiem: " + prefixed);
                            break;

                        case "10":
                            Console.Write("Podaj ścieżkę do pliku CSV: ");
                            string csvPath = Console.ReadLine();
                            CsvImporter.ImportCsvToTable(
                                connectionString,
                                "Units",
                                csvPath,
                                new Dictionary<string, string> { { "Unit", "UnitSI" } }
                            );
                            break;

                        case "0":
                            Console.WriteLine("Wyjście z aplikacji Jednostki.");
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