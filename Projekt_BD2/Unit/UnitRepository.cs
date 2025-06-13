using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using UdtReaderApp.Models; 

namespace UdtReaderApp.Repositories
{
    internal class UnitRepository
    {
        private readonly string _connectionString;

        public UnitRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

 
        public void AddUnit(Unit unit)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Units (Ut) VALUES (@Ut)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlParameter unitParam = new SqlParameter("@Ut", unit.Ut);
                    unitParam.SqlDbType = SqlDbType.Udt;
                    unitParam.UdtTypeName = "UnitSI";
                    command.Parameters.Add(unitParam);
                    command.ExecuteNonQuery();
                }
            }
        }


        public List<Unit> GetAllUnits()
        {
            var units = new List<Unit>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Ut FROM Units";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var unit = new Unit
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Ut = reader.IsDBNull(reader.GetOrdinal("Ut"))
                                    ? UnitSI.Null
                                    : (UnitSI)reader.GetValue(reader.GetOrdinal("Ut"))
                            };
                            units.Add(unit);
                        }
                    }
                }
            }
            return units;
        }


        public Unit GetUnitById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Ut FROM Units WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Unit
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Ut = reader.IsDBNull(reader.GetOrdinal("Ut"))
                                    ? UnitSI.Null
                                    : (UnitSI)reader.GetValue(reader.GetOrdinal("Ut"))
                            };
                        }
                    }
                }
            }
            return null;
        }

      

        public UnitSI AddUnits(int id1, int id2)
        {
            Unit u1 = GetUnitById(id1);
            Unit u2 = GetUnitById(id2);
            return UnitSI.Add(u1?.Ut ?? UnitSI.Null, u2?.Ut ?? UnitSI.Null);
        }

        public UnitSI SubtractUnits(int id1, int id2)
        {
            Unit u1 = GetUnitById(id1);
            Unit u2 = GetUnitById(id2);
            return UnitSI.Subtract(u1?.Ut ?? UnitSI.Null, u2?.Ut ?? UnitSI.Null);
        }

        public UnitSI MultiplyUnits(int id1, int id2)
        {
            Unit u1 = GetUnitById(id1);
            Unit u2 = GetUnitById(id2);
            UnitSI result = UnitSI.Multiply(u1?.Ut ?? UnitSI.Null, u2?.Ut ?? UnitSI.Null);

            if (!result.IsNull)
            {
                AddUnit(new Unit { Ut = result });
            }

            return result;
        }

        public UnitSI DivideUnits(int id1, int id2)
        {
            Unit u1 = GetUnitById(id1);
            Unit u2 = GetUnitById(id2);
            UnitSI result = UnitSI.Divide(u1?.Ut ?? UnitSI.Null, u2?.Ut ?? UnitSI.Null);

            if (!result.IsNull)
            {
                AddUnit(new Unit { Ut = result });
            }

            return result;
        }

        public UnitSI MultiplyUnitByScalar(int id, double scalar)
        {
            Unit u = GetUnitById(id);
            return UnitSI.MultiplyByScalar(u?.Ut ?? UnitSI.Null, scalar);
        }

        public UnitSI DivideUnitByScalar(int id, double scalar)
        {
            Unit u = GetUnitById(id);
            return UnitSI.DivideByScalar(u?.Ut ?? UnitSI.Null, scalar);
        }

        public string GetUnitWithPrefix(int id, string prefix)
        {
            Unit unit = GetUnitById(id);
            if (unit == null || unit.Ut.IsNull)
                return "NULL";

            SqlString result = unit.Ut.ToPrefixedString(new SqlString(prefix));
            return result.IsNull ? "NULL" : result.Value;
        }
    }
}