using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UdtReaderApp.Models;

namespace UdtReaderApp.Repositories
{
    public class VectorRepository
    {
        private readonly string _connectionString;

        public VectorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddVector(Vector vector)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Vectors (Vec) VALUES (@Vec)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    SqlParameter vectorParam = new SqlParameter("@Vec", vector.Vec);
                    vectorParam.SqlDbType = SqlDbType.Udt;
                    vectorParam.UdtTypeName = "Vector3D";

                    command.Parameters.Add(vectorParam);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Vector> GetAllVectors()
        {
            List<Vector> vectors = new List<Vector>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Vec FROM Vectors";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Vector vec = new Vector
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id"))
                            };

                            if (reader.IsDBNull(reader.GetOrdinal("Vec")))
                            {
                                vec.Vec = Vector3D.Null;
                            }
                            else
                            {
                                vec.Vec = (Vector3D)reader.GetValue(reader.GetOrdinal("Vec"));
                            }
                            vectors.Add(vec);
                        }
                    }
                }
            }
            return vectors;
        }

        //Get a Vector3D by vector table ID
        public Vector3D GetVectorById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Vec FROM Vectors WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    object result = command.ExecuteScalar();
                    if (result == null || result is DBNull)
                        return Vector3D.Null;
                    return (Vector3D)result;
                }
            }
        }
        public void DeleteVector(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Vectors WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"Wektor o ID: {id} nie został znaleziony.");
                    }
                    else
                    {
                        Console.WriteLine($"Wektor o ID: {id} został pomyślnie usunięty.");
                    }
                }
            }
        }

        // Add two vectors by their IDs and return the result
        public Vector3D AddVectors(int id1, int id2)
        {
            Vector3D v1 = GetVectorById(id1);
            Vector3D v2 = GetVectorById(id2);
            return Vector3D.Add(v1, v2);
        }

        // Subtract two vectors by their IDs and return the result
        public Vector3D SubtractVectors(int id1, int id2)
        {
            Vector3D v1 = GetVectorById(id1);
            Vector3D v2 = GetVectorById(id2);
            return Vector3D.Subtract(v1, v2);
        }

        // Multiply a vector by a scalar
        public Vector3D MultiplyVectorByScalar(int id, float scalar)
        {
            Vector3D v = GetVectorById(id);
            return Vector3D.MultiplyByScalar(v, scalar);
        }

        // Dot product of two vectors
        public double DotProduct(int id1, int id2)
        {
            Vector3D v1 = GetVectorById(id1);
            Vector3D v2 = GetVectorById(id2);
            var result = Vector3D.DotProduct(v1, v2);
            return result.IsNull ? double.NaN : result.Value;
        }

        // Cross product of two vectors
        public Vector3D CrossProduct(int id1, int id2)
        {
            Vector3D v1 = GetVectorById(id1);
            Vector3D v2 = GetVectorById(id2);
            return Vector3D.CrossProduct(v1, v2);
        }

    }
}