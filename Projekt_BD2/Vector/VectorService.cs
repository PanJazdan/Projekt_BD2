using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class VectorService
    {
        private readonly VectorRepository _vectorRepository;

        public VectorService(string connectionString)
        {
            _vectorRepository = new VectorRepository(connectionString);
        }

        public bool AddVector(string vectorString)
        {
            try
            {
                Vector3D vector = Vector3D.Parse(new SqlString(vectorString));
                var vec = new Vector { Vec = vector };
                _vectorRepository.AddVector(vec);
                Console.WriteLine($"Dodano wektor: {vectorString}");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd walidacji wektora: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania wektora: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public List<Vector> GetAllVectors()
        {
            Console.WriteLine("\n--- Wszystkie wektory ---");
            return _vectorRepository.GetAllVectors();
        }

        public void DeleteVector(int id)
        {
            Console.WriteLine($"\n--- Usuwanie wektora o ID: {id} ---");
            _vectorRepository.DeleteVector(id);
        }

        public void DisplayVectors(List<Vector> vectors)
        {
            if (vectors == null || vectors.Count == 0)
            {
                Console.WriteLine("Brak wektorów do wyświetlenia.");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-10} {2,-10} {3,-10} {4,-20}", "ID", "X", "Y", "Z", "Wektor");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var vector in vectors)
            {
                if (!vector.Vec.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-10} {2,-10} {3,-10} {4,-20}",
                        vector.Id,
                        vector.Vec.x,
                        vector.Vec.y,
                        vector.Vec.z,
                        vector.Vec.ToString());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-10} {2,-10} {3,-10} {4,-20}",
                        vector.Id, "NULL", "NULL", "NULL", "NULL");
                }
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        public Vector3D AddVectors(int id1, int id2)
        {
            return _vectorRepository.AddVectors(id1, id2);
        }

        public Vector3D SubtractVectors(int id1, int id2)
        {
            return _vectorRepository.SubtractVectors(id1, id2);
        }

        public Vector3D MultiplyVectorByScalar(int id, float scalar)
        {
            return _vectorRepository.MultiplyVectorByScalar(id, scalar);
        }

        public double DotProduct(int id1, int id2)
        {
            return _vectorRepository.DotProduct(id1, id2);
        }

        public Vector3D CrossProduct(int id1, int id2)
        {
            return _vectorRepository.CrossProduct(id1, id2);
        }

        public Vector3D GetVectorById(int id)
        {
            return _vectorRepository.GetVectorById(id);
        }
    }
}