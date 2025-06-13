using System;
using System.Collections.Generic;
using System.Globalization;
using UdtReaderApp.Models;
using UdtReaderApp.Services;
using UdtReaderApp.Utils;

namespace UdtReaderApp
{
   
    internal class LocationApp
    {
        private readonly LocationService _locationService;

        public LocationApp(LocationService locationService)
        {
            _locationService = locationService;
        }

        public void Run(string connectionString)
        {
            Console.WriteLine("--- Aplikacja Zarządzania Lokalizacjami (CLR UDT GeoLocation) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nową lokalizację");
                Console.WriteLine("2. Wyświetl wszystkie lokalizacje");
                Console.WriteLine("3. Usuń lokalizację po ID");
                Console.WriteLine("4. Znajdź najbliższą lokalizację do podanej");
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
                            Console.Write("Podaj nazwę lokalizacji: ");
                            string name = Console.ReadLine();
                            Console.Write("Podaj lokalizację w formacie (lat, lon) lub 'latN, lonE' (np. 52.2297N, 21.0122E): ");
                            string geoString = Console.ReadLine();
                            _locationService.AddLocation(name, geoString);
                            break;

                        case "2":
                            _locationService.DisplayLocations(_locationService.GetAllLocations());
                            break;

                        case "3":
                            Console.Write("Podaj ID lokalizacji do usunięcia: ");
                            if (!int.TryParse(Console.ReadLine(), out int deleteId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            _locationService.DeleteLocation(deleteId);
                            break;

                        case "4":
                            Console.Write("Podaj ID lokalizacji referencyjnej: ");
                            if (!int.TryParse(Console.ReadLine(), out int refId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            var refLocation = _locationService.GetLocationById(refId);
                            if (refLocation == null || refLocation.Loc.IsNull)
                            {
                                Console.WriteLine("Nie znaleziono lokalizacji o podanym ID.");
                                break;
                            }
                            var closest = _locationService.FindClosestLocationById(refId);
                            if (closest == null)
                            {
                                Console.WriteLine("Brak innych lokalizacji w bazie.");
                            }
                            else
                            {
                                double dist = GeoLocation.DistanceKm(refLocation.Loc, closest.Loc).Value;
                                Console.WriteLine($"Najbliższa lokalizacja: {closest.Name} (ID: {closest.Id})");
                                Console.WriteLine($"Współrzędne: {closest.Loc.ToString()} / {closest.Loc.ToCardinalString()}");
                                Console.WriteLine($"Odległość: {dist:F3} km");
                            }
                            break;
                        
                        case "5":
                            Console.Write("Podaj ścieżkę do pliku CSV: ");
                            string csvPath = Console.ReadLine();
                            CsvImporter.ImportCsvToTable(
                                connectionString,
                                "Locations",
                                csvPath,
                                new Dictionary<string, string> { { "Location", "GeoLocation" } }
                            );
                            break;


                        case "0":
                            Console.WriteLine("Wyjście z aplikacji Lokalizacje.");
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