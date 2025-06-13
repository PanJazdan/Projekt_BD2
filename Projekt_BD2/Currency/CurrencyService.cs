using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UdtReaderApp.Models;
using UdtReaderApp.Repositories;

namespace UdtReaderApp.Services
{
    public class CurrencyService
    {
        private readonly CurrencyRepository _currencyRepository;

        public CurrencyService(string connectionString)
        {
            _currencyRepository = new CurrencyRepository(connectionString);
        }

        public bool AddCurrency(string moneyString)
        {
            try
            {
                MoneyType money = MoneyType.Parse(new SqlString(moneyString));
                var currency = new Currency { MoneyValue = money };
                _currencyRepository.AddCurrency(currency);
                Console.WriteLine($"Dodano wartość: {money.ToString()}");
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd formatu wartości pieniężnej: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Błąd podczas dodawania wartości pieniężnej: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public List<Currency> GetAllCurrencies()
        {
            Console.WriteLine("\n--- Wszystkie wartości pieniężne ---");
            return _currencyRepository.GetAllCurrencies();
        }

        public void DisplayCurrencies(List<Currency> currencies)
        {
            if (currencies == null || currencies.Count == 0)
            {
                Console.WriteLine("Brak wartości do wyświetlenia.");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20}", "ID", "Wartość");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var currency in currencies)
            {
                if (!currency.MoneyValue.IsNull)
                {
                    Console.WriteLine("{0,-5} {1,-20}", currency.Id, currency.MoneyValue.ToString());
                }
                else
                {
                    Console.WriteLine("{0,-5} {1,-20}", currency.Id, "NULL");
                }
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        public void DeleteCurrency(int id)
        {
            Console.WriteLine($"\n--- Usuwanie wartości o ID: {id} ---");
            _currencyRepository.DeleteCurrency(id);
        }

        public Currency GetCurrencyById(int id)
        {
            return _currencyRepository.GetCurrencyById(id);
        }

        public MoneyType ConvertCurrency(int id, string targetCurrency)
        {
            var currency = GetCurrencyById(id);
            if (currency == null || currency.MoneyValue.IsNull)
                throw new ArgumentException($"Wartość o ID {id} nie istnieje lub jest pusta.");

            return currency.MoneyValue.ConvertTo(targetCurrency);
        }
    }
}