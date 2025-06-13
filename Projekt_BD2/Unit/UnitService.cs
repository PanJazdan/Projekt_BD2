using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class UnitService
    {
        private readonly UnitRepository _unitRepository;

        public UnitService(string connectionString)
        {
            _unitRepository = new UnitRepository(connectionString);
        }

        public bool AddUnit(string unitString)
        {
            try
            {
                UnitSI unitSI = UnitSI.Parse(new SqlString(unitString));
                var unit = new Unit { Ut = unitSI };
                _unitRepository.AddUnit(unit);
                Console.WriteLine($"Dodano jednostkę: {unitString}");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd walidacji jednostki: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania jednostki: {ex}");
                Console.ResetColor();
                return false;
            }
        }

        public List<Unit> GetAllUnits()
        {
            Console.WriteLine("\n--- Wszystkie jednostki ---");
            return _unitRepository.GetAllUnits();
        }

        public void DisplayUnits(List<Unit> units)
        {
            if (units == null || units.Count == 0)
            {
                Console.WriteLine("Brak jednostek do wyświetlenia.");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20}", "ID", "Jednostka");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var unit in units)
            {
                if (!unit.Ut.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-20}", unit.Id, unit.Ut.ToString());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-20}", unit.Id, "NULL");
                }
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        public UnitSI AddUnits(int id1, int id2)
        {
            return _unitRepository.AddUnits(id1, id2);
        }

        public UnitSI SubtractUnits(int id1, int id2)
        {
            return _unitRepository.SubtractUnits(id1, id2);
        }

        public UnitSI MultiplyUnits(int id1, int id2)
        {
            return _unitRepository.MultiplyUnits(id1, id2);
        }

        public UnitSI DivideUnits(int id1, int id2)
        {
            return _unitRepository.DivideUnits(id1, id2);
        }

        public UnitSI MultiplyUnitByScalar(int id, double scalar)
        {
            return _unitRepository.MultiplyUnitByScalar(id, scalar);
        }

        public UnitSI DivideUnitByScalar(int id, double scalar)
        {
            return _unitRepository.DivideUnitByScalar(id, scalar);
        }

        public string GetUnitWithPrefix(int id, string prefix)
        {
            return _unitRepository.GetUnitWithPrefix(id, prefix);
        }

        public Unit GetUnitById(int id)
        {
            return _unitRepository.GetUnitById(id);
        }
    }
}