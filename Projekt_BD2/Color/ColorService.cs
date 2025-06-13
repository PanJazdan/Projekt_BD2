using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class ColorService
    {
        private readonly ColorRepository _colorRepository;

        public ColorService(string connectionString)
        {
            _colorRepository = new ColorRepository(connectionString);
        }

        public bool AddColor(string colorString)
        {
            try
            {
                ColorRGB color = ColorRGB.Parse(new SqlString(colorString));
                var c = new Color { ColorValue = color };
                _colorRepository.AddColor(c);
                Console.WriteLine($"Dodano kolor: {color.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania koloru: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public List<Color> GetAllColors()
        {
            Console.WriteLine("\n--- Wszystkie kolory ---");
            return _colorRepository.GetAllColors();
        }

        public void DisplayColors(List<Color> colors)
        {
            if (colors == null || colors.Count == 0)
            {
                Console.WriteLine("Brak kolorów do wyświetlenia.");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-10}", "ID", "RGB", "HEX");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var color in colors)
            {
                if (!color.ColorValue.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-10}",
                        color.Id,
                        color.ColorValue.ToString(),
                        color.ColorValue.ToHex());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-10}",
                        color.Id, "NULL", "NULL");
                }
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        public void DeleteColor(int id)
        {
            Console.WriteLine($"\n--- Usuwanie koloru o ID: {id} ---");
            _colorRepository.DeleteColor(id);
        }

        public Color GetColorById(int id)
        {
            return _colorRepository.GetColorById(id);
        }

        public Color ColorNegate(int id)
        {
            return _colorRepository.Negate(id);
        }

        public Color BlendColors(int id1, int id2, double ratio)
        {
            return _colorRepository.Blend(id1, id2, ratio);
        }
    }
}