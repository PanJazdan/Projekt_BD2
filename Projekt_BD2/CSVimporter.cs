using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using UdtReaderApp.Models;

namespace UdtReaderApp.Utils
{
    public static class CsvImporter
    {
        public static void ImportCsvToTable(string connectionString, string tableName, string csvPath, Dictionary<string, string> udtColumnTypes = null)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Plik CSV nie istnieje: {csvPath}");
                return;
            }

            using (var reader = new StreamReader(csvPath))
            {
                string headerLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    Console.WriteLine("Plik CSV nie zawiera nagłówka.");
                    return;
                }

                string[] columns = headerLine.Split(',');
                int colCount = columns.Length;

                string insertSql = $"INSERT INTO {tableName} ({string.Join(",", columns)}) VALUES ({string.Join(",", GetParameterNames(columns))})";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int rowNum = 1;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        rowNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] values = SplitCsvLine(line, colCount);
                        if (values.Length != colCount)
                        {
                            Console.WriteLine($"Błąd w wierszu {rowNum}: liczba kolumn nie zgadza się z nagłówkiem.");
                            continue;
                        }

                        using (var cmd = new SqlCommand(insertSql, connection))
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                string col = columns[i].Trim();
                                string val = values[i].Trim();

                                if (udtColumnTypes != null && udtColumnTypes.TryGetValue(col, out string udtType))
                                {
                                    object udtValue = null;
                                    switch (udtType)
                                    {
                                        case "GeoLocation":
                                            udtValue = GeoLocation.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        case "UnitSI":
                                            udtValue = UnitSI.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        case "Vector3D":
                                            udtValue = Vector3D.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        case "Email":
                                            udtValue = Email.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        case "ColorRGB":
                                            udtValue = ColorRGB.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        case "MoneyType":
                                            udtValue = MoneyType.Parse(new System.Data.SqlTypes.SqlString(val));
                                            break;
                                        default:
                                            throw new NotSupportedException($"UDT type {udtType} is not supported.");
                                    }
                                    var param = new SqlParameter("@" + col, udtValue);
                                    param.SqlDbType = SqlDbType.Udt;
                                    param.UdtTypeName = udtType;
                                    cmd.Parameters.Add(param);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@" + col, string.IsNullOrEmpty(val) ? (object)DBNull.Value : val);
                                }
                            }

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Błąd w wierszu {rowNum}: {ex.Message}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Import CSV zakończony.");
        }

        private static string[] GetParameterNames(string[] columns)
        {
            string[] names = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
                names[i] = "@" + columns[i].Trim();
            return names;
        }


        private static string[] SplitCsvLine(string line, int expectedColumns)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string current = "";
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);
            while (result.Count < expectedColumns)
                result.Add("");
            return result.ToArray();
        }
    }
}