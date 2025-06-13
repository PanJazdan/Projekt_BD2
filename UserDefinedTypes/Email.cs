using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server; 
using Microsoft.SqlServer.Types;

[Serializable]

[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize =128, ValidationMethodName = "ValidateEmailUdt")]
public struct Email : INullable, IBinarySerialize
{
    private bool is_Null;
    public string username;
    public string domain;


    public bool IsNull => is_Null;


    public static Email Null => new Email { is_Null = true };

    [SqlMethod(OnNullCall = false)] 
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


    public bool ValidateEmailUdt()
    {
        if (this.is_Null) return true;
        return IsValidLocalPart(this.username) && IsValidDomain(this.domain);
    }


    [SqlMethod(OnNullCall = false)] 
    public override string ToString()
    {
        if (this.IsNull) return "NULL";
        return $"{username}@{domain}";
    }


    public void Read(BinaryReader r)
    {
        is_Null = r.ReadBoolean(); 
        if (!is_Null)
        {
            username = r.ReadString();
            domain = r.ReadString();
        }
    }

    public void Write(BinaryWriter w)
    {
        w.Write(is_Null); 
        if (!is_Null)
        {
            w.Write(username ?? string.Empty); 
            w.Write(domain ?? string.Empty);
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Email other = (Email)obj;
        if (is_Null && other.is_Null) return true; 
        if (is_Null != other.is_Null) return false; 

 
        return username.Equals(other.username, StringComparison.OrdinalIgnoreCase) &&
               domain.Equals(other.domain, StringComparison.OrdinalIgnoreCase);
    }


    public override int GetHashCode()
    {
        if (is_Null) return 0; 

    
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (username != null ? username.ToLowerInvariant().GetHashCode() : 0);
            hash = hash * 23 + (domain != null ? domain.ToLowerInvariant().GetHashCode() : 0);
            return hash;
        }
    }
}