using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UdtReaderApp.Models;

namespace UdtReaderApp.Repositories
{
    public class ColorRepository
    {
        private readonly string _connectionString;

        public ColorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddColor(Color color)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Colors (ColorValue) VALUES (@ColorValue)";
                using (var command = new SqlCommand(query, connection))
                {
                    var param = new SqlParameter("@ColorValue", color.ColorValue);
                    param.SqlDbType = SqlDbType.Udt;
                    param.UdtTypeName = "ColorRGB";
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Color> GetAllColors()
        {
            var colors = new List<Color>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ColorValue FROM Colors";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            colors.Add(new Color
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ColorValue = reader.IsDBNull(reader.GetOrdinal("ColorValue"))
                                    ? ColorRGB.Null
                                    : (ColorRGB)reader.GetValue(reader.GetOrdinal("ColorValue"))
                            });
                        }
                    }
                }
            }
            return colors;
        }

        public void DeleteColor(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Colors WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Color GetColorById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ColorValue FROM Colors WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Color
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ColorValue = reader.IsDBNull(reader.GetOrdinal("ColorValue"))
                                    ? ColorRGB.Null
                                    : (ColorRGB)reader.GetValue(reader.GetOrdinal("ColorValue"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Color Negate(int id)
        {
            Color color = GetColorById(id);
            if (color == null || color.ColorValue.IsNull)
            {
                throw new ArgumentException($"Kolor o ID {id} nie istnieje lub jest pusty.");
            }
            ColorRGB negatedColor = color.ColorValue.Negate();
            return new Color
            {
                Id = color.Id,
                ColorValue = negatedColor
            };
        }

        public Color Blend(int id1, int id2, double ratio)
        {
            Color color1 = GetColorById(id1);
            Color color2 = GetColorById(id2);

            if (color1 == null || color1.ColorValue.IsNull)
                throw new ArgumentException($"Kolor o ID {id1} nie istnieje lub jest pusty.");
            if (color2 == null || color2.ColorValue.IsNull)
                throw new ArgumentException($"Kolor o ID {id2} nie istnieje lub jest pusty.");

            ColorRGB blended = color1.ColorValue.Blend(color2.ColorValue, ratio);
            return new Color
            {
                Id = 0, 
                ColorValue = blended
            };
        }
    }
}