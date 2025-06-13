using System;
using System.Collections.Generic;
using System.Globalization;
using UdtReaderApp.Models;
using UdtReaderApp.Services;
using UdtReaderApp.Utils;

namespace UdtReaderApp
{
    // Klasa VectorApp zarządza interakcją z użytkownikiem dla typu Vector3D
    public class VectorApp
    {
        private readonly VectorService _vectorService;

        // Konstruktor przyjmuje VectorService (wstrzyknięcie zależności)
        public VectorApp(VectorService vectorService)
        {
            _vectorService = vectorService;
        }

        // Metoda Run zawiera całą logikę menu i interakcji dla Vector3D
        public void Run(string connectionString)
        {
            Console.WriteLine("--- Aplikacja Zarządzania Wektorami (CLR UDT Vector3D) ---");

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1. Dodaj nowy wektor");
                Console.WriteLine("2. Wyświetl wszystkie wektory");
                Console.WriteLine("3. Usuń wektor po ID");
                Console.WriteLine("4. Dodaj dwa wektory (podaj ID)");
                Console.WriteLine("5. Odejmij dwa wektory (podaj ID)");
                Console.WriteLine("6. Mnożenie wektora przez skalar");
                Console.WriteLine("7. Iloczyn skalarny (dot product) dwóch wektorów");
                Console.WriteLine("8. Iloczyn wektorowy (cross product) dwóch wektorów");
                Console.WriteLine("9. Dodaj rekordy przez plik CSV");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.Write("Podaj wektor w formacie [x,y,z] (np. [1.5,2.0,3.0]): ");
                            string newVector = Console.ReadLine();
                            _vectorService.AddVector(newVector);
                            break;

                        case "2":
                            _vectorService.DisplayVectors(_vectorService.GetAllVectors());
                            break;

                        case "3":
                            Console.Write("Podaj ID wektora do usunięcia: ");
                            if (!int.TryParse(Console.ReadLine(), out int deleteId))
                            {
                                Console.WriteLine("Nieprawidłowe ID. Podaj liczbę.");
                                break;
                            }
                            _vectorService.DeleteVector(deleteId);
                            break;

                        case "4":
                            Console.Write("Podaj ID pierwszego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int id1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int id2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var sum = _vectorService.AddVectors(id1, id2);
                            Console.WriteLine($"Suma: {sum}");
                            break;

                        case "5":
                            Console.Write("Podaj ID pierwszego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int sid1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int sid2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var diff = _vectorService.SubtractVectors(sid1, sid2);
                            Console.WriteLine($"Różnica: {diff}");
                            break;

                        case "6":
                            Console.Write("Podaj ID wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int mid))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj skalar (liczba zmiennoprzecinkowa): ");
                            string scalarStr = Console.ReadLine();
                            if (!float.TryParse(scalarStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float scalar))
                            {
                                Console.WriteLine("Nieprawidłowy skalar.");
                                break;
                            }
                            var scaled = _vectorService.MultiplyVectorByScalar(mid, scalar);
                            Console.WriteLine($"Wynik mnożenia: {scaled}");
                            break;

                        case "7":
                            Console.Write("Podaj ID pierwszego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int did1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int did2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var dot = _vectorService.DotProduct(did1, did2);
                            Console.WriteLine($"Iloczyn skalarny: {dot}");
                            break;

                        case "8":
                            Console.Write("Podaj ID pierwszego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int cid1))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            Console.Write("Podaj ID drugiego wektora: ");
                            if (!int.TryParse(Console.ReadLine(), out int cid2))
                            {
                                Console.WriteLine("Nieprawidłowe ID.");
                                break;
                            }
                            var cross = _vectorService.CrossProduct(cid1, cid2);
                            Console.WriteLine($"Iloczyn wektorowy: {cross}");
                            break;

                        case "9":
                            Console.Write("Podaj ścieżkę do pliku CSV: ");
                            string csvPath = Console.ReadLine();
                            CsvImporter.ImportCsvToTable(
                                connectionString,
                                "Vectors",
                                csvPath,
                                new Dictionary<string, string> { { "Vector", "Vector3D" } }
                            );
                            break;

                        case "0":
                            Console.WriteLine("Zamykanie aplikacji...");
                            return;

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
