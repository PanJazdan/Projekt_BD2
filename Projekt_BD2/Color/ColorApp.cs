using System;
using System.Collections.Generic;
using System.Globalization;
using UdtReaderApp.Models;
using UdtReaderApp.Services;
using UdtReaderApp.Utils;

namespace UdtReaderApp
{
    internal class ColorApp
    {
        private readonly ColorService _colorService;

        public ColorApp(ColorService colorService)
        {
            _colorService = colorService;
        }

        public void Run(string connectionString)
        {
            Console.WriteLine("--- Aplikacja Zarządzania Kolorami (CLR UDT ColorRGB) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nowy kolor");
                Console.WriteLine("2. Wyświetl wszystkie kolory");
                Console.WriteLine("3. Usuń kolor po ID");
                Console.WriteLine("4. Oblicz negatyw koloru");
                Console.WriteLine("5. Zblenduj dwa kolory");
                Console.WriteLine("6. Dodaj rekordy przez plik CSV");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.Write("Podaj kolor w formacie [R,G,B] lub #RRGGBB lub #RRGGBBAA: ");
                            string colorString = Console.ReadLine();
                            _colorService.AddColor(colorString);
                            break;

                        case "2":
                            _colorService.DisplayColors(_colorService.GetAllColors());
                            break;

                        case "3":
                            Console.Write("Podaj ID koloru do usunięcia: ");
                            if (!int.TryParse(Console.ReadLine(), out int deleteId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            _colorService.DeleteColor(deleteId);
                            break;

                        case "4":
                            Console.Write("Podaj ID koloru: ");
                            if (!int.TryParse(Console.ReadLine(), out int id))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            try
                            {
                                Color c = _colorService.ColorNegate(id);
                                Console.WriteLine($"Zanegowany kolor: : {c.ColorValue.ToString()}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Błąd: " + ex.Message);
                            }
                            break;

                        case "5":
                            Console.Write("Podaj ID pierwszego koloru: ");
                            if (!int.TryParse(Console.ReadLine(), out int blendId1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiego koloru: ");
                            if (!int.TryParse(Console.ReadLine(), out int blendId2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj współczynnik blendowania (0.0 - 1.0): ");
                            if (!double.TryParse(Console.ReadLine(), NumberStyles.Float, CultureInfo.InvariantCulture, out double ratio) || ratio < 0.0 || ratio > 1.0)
                            {
                                Console.WriteLine("Nieprawidłowy współczynnik. Podaj liczbę z zakresu 0.0 - 1.0.");
                                break;
                            }
                            try
                            {
                                Color blended = _colorService.BlendColors(blendId1, blendId2, ratio);
                                Console.WriteLine($"Zblendowany kolor: {blended.ColorValue.ToString()} (HEX: {blended.ColorValue.ToHex()})");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Błąd: " + ex.Message);
                            }
                            break;

                        case "6":
                            Console.Write("Podaj ścieżkę do pliku CSV: ");
                            string csvPath = Console.ReadLine();
                            CsvImporter.ImportCsvToTable(
                                connectionString,
                                "Colors",
                                csvPath,
                                new Dictionary<string, string> { { "ColorValue", "ColorRGB" } }
                            );
                            break;

                        case "0":
                            Console.WriteLine("Wyjście z aplikacji Kolory.");
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