using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UdtReaderApp.Models;

namespace UdtReaderApp.Repositories
{
    public class LocationRepository
    {
        private readonly string _connectionString;

        public LocationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddLocation(Location location)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Locations (Place, Location) VALUES (@Place, @Location)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Place", location.Name);

                    SqlParameter locParam = new SqlParameter("@Location", location.Loc);
                    locParam.SqlDbType = SqlDbType.Udt;
                    locParam.UdtTypeName = "GeoLocation";
                    command.Parameters.Add(locParam);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Location> GetAllLocations()
        {
            var locations = new List<Location>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Place, Location FROM Locations";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var loc = new Location
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Place")),
                                Loc = reader.IsDBNull(reader.GetOrdinal("Location"))
                                    ? GeoLocation.Null
                                    : (GeoLocation)reader.GetValue(reader.GetOrdinal("Location"))
                            };
                            locations.Add(loc);
                        }
                    }
                }
            }
            return locations;
        }

        public Location GetLocationById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Place, Location FROM Locations WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Location
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Place")),
                                Loc = reader.IsDBNull(reader.GetOrdinal("Location"))
                                    ? GeoLocation.Null
                                    : (GeoLocation)reader.GetValue(reader.GetOrdinal("Location"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void DeleteLocation(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Locations WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"Lokalizacja o ID: {id} nie została znaleziona.");
                    }
                    else
                    {
                        Console.WriteLine($"Lokalizacja o ID: {id} została pomyślnie usunięta.");
                    }
                }
            }
        }
    }
}