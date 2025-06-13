using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

[Serializable]
[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 64, ValidationMethodName = "ValidateMoney")]
public struct MoneyType : INullable, IBinarySerialize
{
    private bool is_Null;
    public decimal Amount;
    public string Currency;

    private static readonly List<string> IsoCurrencies = new List<string>()
    {
        "PLN", "EUR", "USD", "GBP", "CHF", "JPY", "CNY"
    };

    private static readonly Dictionary<string, decimal> ToPLN = new Dictionary<string, decimal>()
    {
        {"PLN", 1m},
        {"EUR", 4.5m},
        {"USD", 4.0m},
        {"GBP", 5.0m},
        {"CHF", 4.7m},
        {"JPY", 0.03m},
        {"CNY", 0.6m}
    };

    public bool IsNull => is_Null;
    public static MoneyType Null => new MoneyType { is_Null = true };

    public MoneyType(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency?.ToUpperInvariant() ?? "PLN";
        is_Null = false;
        if (!ValidateCurrency(Currency))
            throw new ArgumentException("Invalid ISO currency code.");
    }

    public static bool ValidateCurrency(string code)
    {
        return !string.IsNullOrEmpty(code) && IsoCurrencies.Contains(code.ToUpperInvariant());
    }

    private bool ValidateMoney()
    {
        if (IsNull) return true;
        return ValidateCurrency(Currency);
    }

    public MoneyType ConvertTo(string targetCurrency)
    {
        if (!ValidateCurrency(targetCurrency))
            throw new ArgumentException("Invalid target ISO currency code.");

        if (!ToPLN.ContainsKey(Currency) || !ToPLN.ContainsKey(targetCurrency))
            throw new ArgumentException("Unsupported currency for conversion.");

        decimal amountInPLN = Amount * ToPLN[Currency];
        decimal result = amountInPLN / ToPLN[targetCurrency];
        return new MoneyType(decimal.Round(result, 2), targetCurrency.ToUpperInvariant());
    }

    public override string ToString()
    {
        if (IsNull) return "NULL";
        return $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} [{Currency}]";
    }

    public static MoneyType Parse(SqlString s)
    {
        if (s.IsNull) return Null;
        string value = s.Value.Trim();
        int bracketIndex = value.IndexOf('[');
        int lastBracketIndex = value.IndexOf(']');
        if (bracketIndex == -1 || lastBracketIndex == -1 || lastBracketIndex <= bracketIndex)
            throw new ArgumentException("Invalid format. Expected 'amount [CURRENCY]'.");

        string numPart = value.Substring(0, bracketIndex).Trim();
        string currencyPart = value.Substring(bracketIndex + 1, lastBracketIndex - bracketIndex - 1).Trim();

        if (!decimal.TryParse(numPart, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
            throw new ArgumentException("Invalid amount.");

        if (!ValidateCurrency(currencyPart))
            throw new ArgumentException("Invalid ISO currency code.");

        return new MoneyType(amount, currencyPart);
    }

    public static MoneyType Add(MoneyType m1, MoneyType m2)
    {
        if (m1.IsNull || m2.IsNull) return Null;
        if (!m1.Currency.Equals(m2.Currency, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Currencies must match for addition.");
        return new MoneyType(m1.Amount + m2.Amount, m1.Currency);
    }

    public static MoneyType Subtract(MoneyType m1, MoneyType m2)
    {
        if (m1.IsNull || m2.IsNull) return Null;
        if (!m1.Currency.Equals(m2.Currency, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Currencies must match for subtraction.");
        return new MoneyType(m1.Amount - m2.Amount, m1.Currency);
    }

    public static MoneyType MultiplyByScalar(MoneyType m, decimal scalar)
    {
        if (m.IsNull) return Null;
        return new MoneyType(m.Amount * scalar, m.Currency);
    }

    public void Write(BinaryWriter w)
    {
        w.Write(is_Null);
        if (!is_Null)
        {
            w.Write(Amount);
            w.Write(Currency ?? string.Empty);
        }
    }

    public void Read(BinaryReader r)
    {
        is_Null = r.ReadBoolean();
        if (!is_Null)
        {
            Amount = r.ReadDecimal();
            Currency = r.ReadString();
        }
        else
        {
            Amount = 0;
            Currency = "PLN";
        }
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MoneyType)) return false;
        var other = (MoneyType)obj;
        return Amount == other.Amount && string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase) && IsNull == other.IsNull;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Amount.GetHashCode();
            hash = hash * 23 + (Currency != null ? Currency.ToUpperInvariant().GetHashCode() : 0);
            return hash;
        }
    }
}