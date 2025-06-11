using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server; // Niezbędne dla atrybutów CLR UDT
using Microsoft.SqlServer.Types;

[Serializable]
// Zmieniamy Format.Native na Format.UserDefined dla zmiennych stringów.
// MaxByteSize powinno być wystarczająco duże, aby pomieścić zserializowany obiekt (np. 8000 jest bezpieczną górną granicą dla varbinary(max))
[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 8000, ValidationMethodName = "ValidateEmailUdt")]
public struct Email : INullable, IBinarySerialize
{
    private bool is_Null;
    public string username;
    public string domain;

    // Implementacja INullable
    public bool IsNull => is_Null;

    // Statyczna właściwość zwracająca instancję NULL
    public static Email Null => new Email { is_Null = true };

    // Metoda Parse: Konwertuje SqlString (z SQL Server) na Email UDT
    [SqlMethod(OnNullCall = false)] // Ta metoda nie będzie wywoływana, jeśli wejściowy SqlString jest NULL
    public static Email Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        string value = s.Value.Trim();

        int atIndex = value.IndexOf('@');

        if (atIndex <= 0 || atIndex != value.LastIndexOf('@') || atIndex == value.Length - 1)
            throw new ArgumentException("Input must contain exactly one '@' separating username and domain.");

        string username = value.Substring(0, atIndex);
        string domain = value.Substring(atIndex + 1);

        if (!IsValidLocalPart(username))
            throw new ArgumentException("Invalid email username (before @).");
        if (!IsValidDomain(domain))
            throw new ArgumentException("Invalid email domain (after @).");

        return new Email
        {
            username = username.ToLowerInvariant(),
            domain = domain.ToLowerInvariant(),
            is_Null = false
        };
    }

    // Walidacja lokalnej części adresu e-mail (przed '@')
    private static bool IsValidLocalPart(string local)
    {
        if (string.IsNullOrEmpty(local) || local.Length > 64)
            return false;

        if (local.StartsWith(".") || local.EndsWith(".") || local.Contains("..") || local.Contains("\""))
            return false;

        foreach (char c in local)
        {
            if (!(char.IsLetterOrDigit(c) || "!#$%&'*+-/=?^_`{|}~.".Contains(c.ToString())))
                return false;
        }
        return true;
    }

    // Walidacja domeny (po '@')
    private static bool IsValidDomain(string domain)
    {
        if (string.IsNullOrEmpty(domain) || domain.Length > 255)
            return false;

        string[] labels = domain.Split('.');
        if (labels.Length < 2)
            return false;

        foreach (string label in labels)
        {
            if (label.Length == 0 || label.Length > 63)
                return false;
            if (label.StartsWith("-") || label.EndsWith("-"))
                return false;
            foreach (char c in label)
            {
                if (!(char.IsLetterOrDigit(c) || c == '-'))
                    return false;
            }
        }
        return true;
    }

    // Dodana metoda walidacji dla CLR UDT, wywoływana przez SQL Server
    public bool ValidateEmailUdt()
    {
        if (this.is_Null) return true; // NULL jest zawsze walidowany jako poprawny
        return IsValidLocalPart(this.username) && IsValidDomain(this.domain);
    }


    [SqlMethod(OnNullCall = false)] // Opcjonalnie: nie wywołuj, jeśli instancja jest NULL
    public override string ToString()
    {
        if (this.IsNull) return "NULL";
        return $"{username}@{domain}";
    }

    // Implementacja IBinarySerialize: Odczyt i zapis danych dla SQL Server
    // Te metody są wywoływane przez SQL Server, gdy używasz Format.UserDefined
    public void Read(BinaryReader r)
    {
        is_Null = r.ReadBoolean(); // Najpierw odczytujemy flagę null
        if (!is_Null)
        {
            username = r.ReadString();
            domain = r.ReadString();
        }
    }

    public void Write(BinaryWriter w)
    {
        w.Write(is_Null); // Zapisujemy flagę null
        if (!is_Null)
        {
            w.Write(username ?? string.Empty); // Zapisujemy string, obsługując null
            w.Write(domain ?? string.Empty);
        }
    }

    // --- ZMIENIONA METODA GET HASH CODE ---
    // Implementacja Equals jest kluczowa dla typów wartości
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Email other = (Email)obj;
        if (is_Null && other.is_Null) return true; // Dwa nulle są sobie równe
        if (is_Null != other.is_Null) return false; // Null i nie-null nie są sobie równe

        // Porównujemy bez uwzględniania wielkości liter (zgodnie z normalizacją)
        return username.Equals(other.username, StringComparison.OrdinalIgnoreCase) &&
               domain.Equals(other.domain, StringComparison.OrdinalIgnoreCase);
    }

    // Implementacja GetHashCode() dla kompatybilności z .NET Framework
    public override int GetHashCode()
    {
        if (is_Null) return 0; // Hash code dla NULL

        // Standardowa implementacja GetHashCode dla wielu pól w .NET Framework
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (username != null ? username.ToLowerInvariant().GetHashCode() : 0);
            hash = hash * 23 + (domain != null ? domain.ToLowerInvariant().GetHashCode() : 0);
            return hash;
        }
    }

    [SqlMethod(OnNullCall = false)]
    public bool IsCorporateDomain()
    {
        if (this.IsNull) return false;
        return domain.EndsWith(".com") || domain.EndsWith(".org") || domain.EndsWith(".net");
    }
}