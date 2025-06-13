using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class LocationService
    {
        private readonly LocationRepository _locationRepository;

        public LocationService(string connectionString)
        {
            _locationRepository = new LocationRepository(connectionString);
        }

        public bool AddLocation(string name, string geoString)
        {
            try
            {
                GeoLocation location = GeoLocation.Parse(new SqlString(geoString));
                var loc = new Location { Name = name, Loc = location };
                _locationRepository.AddLocation(loc);
                Console.WriteLine($"Dodano lokalizację: {name} {location.ToString()}");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd walidacji lokalizacji: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania lokalizacji: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public List<Location> GetAllLocations()
        {
            Console.WriteLine("\n--- Wszystkie lokalizacje ---");
            return _locationRepository.GetAllLocations();
        }

        public void DisplayLocations(List<Location> locations)
        {
            if (locations == null || locations.Count == 0)
            {
                Console.WriteLine("Brak lokalizacji do wyświetlenia.");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-30} {3,-30}", "ID", "Nazwa", "GeoLocation", "GeoLocation (N/S, E/W)");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var loc in locations)
            {
                if (!loc.Loc.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-30} {3,-30}",
                        loc.Id,
                        loc.Name,
                        loc.Loc.ToString(),
                        loc.Loc.ToCardinalString());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-30} {3,-30}",
                        loc.Id, loc.Name, "NULL", "NULL");
                }
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        public Location GetLocationById(int id)
        {
            return _locationRepository.GetLocationById(id);
        }

        /// <summary>
        /// Finds the closest location in the table to the given GeoLocation.
        /// Returns null if the table is empty.
        /// </summary>
        public Location FindClosestLocationById(int referenceId)
        {
            var reference = GetLocationById(referenceId);
            if (reference == null || reference.Loc.IsNull)
                return null;

            var locations = _locationRepository.GetAllLocations();
            Location closest = null;
            double minDistance = double.MaxValue;

            foreach (var loc in locations)
            {
                if (loc.Id == referenceId || loc.Loc.IsNull)
                    continue;

                double dist = GeoLocation.DistanceKm(reference.Loc, loc.Loc).Value;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = loc;
                }
            }

            return closest;
        }

        public void DeleteLocation(int id)
        {
            Console.WriteLine($"\n--- Usuwanie lokalizacji o ID: {id} ---");
            _locationRepository.DeleteLocation(id);
        }
    }
}