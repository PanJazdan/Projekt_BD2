﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;

[Serializable]
[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 256, ValidationMethodName = "ValidateUnitSI")]
public struct UnitSI : INullable, IBinarySerialize
{
    private bool is_Null;

    public double Value;

    public string Unit; 

    
    private static Dictionary<string, double> GetPrefixes()
    {
        return new Dictionary<string, double>(StringComparer.Ordinal)
    {
        {"Y", 1e24}, {"Z", 1e21}, {"E", 1e18}, {"P", 1e15},
        {"T", 1e12}, {"G", 1e9}, {"M", 1e6}, {"k", 1e3},
        {"h", 1e2}, {"da", 1e1}, {"", 1e0}, {"d", 1e-1},
        {"c", 1e-2}, {"m", 1e-3}, {"u", 1e-6},
        {"n", 1e-9}, {"p", 1e-12}, {"f", 1e-15}, {"a", 1e-18},
        {"z", 1e-21}, {"y", 1e-24}
    };
    }

    private static List<string> GetBaseSiUnits()
    {
        return new List<string>()
    {
        "m", "kg", "s", "A", "K", "mol", "cd"
    };
    }


    public bool IsNull
    {
        get { return is_Null; }
    }

    public static UnitSI Null
    {
        get
        {
            UnitSI u = new UnitSI();
            u.is_Null = true;
            u.Value = 0;
            u.Unit = "none"; 
            return u;
        }
    }

   
    public UnitSI(double value, string unit)
    {
       
        this.Unit = string.IsNullOrEmpty(unit) ? "none" : unit;
        this.Value = value;
        this.is_Null = false;

        if(double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("Value cannot be NaN or Infinity.");
        }

        if(!ValidateUnitSI())
        {
            throw new ArgumentException("Invalid UnitSI object. Validation failed.");
        }
     
    }


    private bool ValidateUnitSI()
    {
     
        if (this.IsNull) return true;

        return true;
    }


    [SqlMethod(OnNullCall = false)]
    public static UnitSI Parse(SqlString s)
    {
        if (s.IsNull) return Null;

        string value = s.Value.Trim();
        int bracketIndex = value.IndexOf('[');
        int lastBracketIndex = value.LastIndexOf(']');

        
        if (bracketIndex == -1 || lastBracketIndex == -1 || lastBracketIndex <= bracketIndex)
        {
            throw new ArgumentException("Invalid format for UnitSI. Expected 'value [unit]'.");
        }

        string numPart = value.Substring(0, bracketIndex).Trim();
        string unitPart = value.Substring(bracketIndex + 1, lastBracketIndex - bracketIndex - 1).Trim();

      
        if (string.IsNullOrEmpty(unitPart))
        {
            throw new ArgumentException("Unit part cannot be empty in UnitSI string. Expected 'value [unit]'.");
        }

        double val;
        
        if (!double.TryParse(numPart, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
        {
           
            throw new ArgumentException("Invalid numeric value in UnitSI string.");
        }

        var baseSiUnits = GetBaseSiUnits();
        if (!baseSiUnits.Contains(unitPart))
        {
            throw new ArgumentException($"Invalid unit '{unitPart}' in UnitSI string. Expected one of: {string.Join(", ", baseSiUnits)}.");
        }



        return new UnitSI(val, unitPart);
    }

    [SqlMethod(OnNullCall = false)]
    public override string ToString()
    {
        if (is_Null)
            return "NULL";

        return $"{Value.ToString(CultureInfo.InvariantCulture)} [{Unit}]";
    }

  
    [SqlMethod(OnNullCall = false)]
    public static UnitSI Add(UnitSI u1, UnitSI u2)
    {
        if (u1.IsNull || u2.IsNull) return Null;
    
        if (!u1.Unit.Equals(u2.Unit, StringComparison.Ordinal))
        {
            throw new ArgumentException("Units must be identical for addition.");
        }
        return new UnitSI(u1.Value + u2.Value, u1.Unit);
    }

    [SqlMethod(OnNullCall = false)]
    public static UnitSI Subtract(UnitSI u1, UnitSI u2)
    {
        if (u1.IsNull || u2.IsNull) return Null;
        if (!u1.Unit.Equals(u2.Unit, StringComparison.Ordinal))
        {
            throw new ArgumentException("Units must be identical for subtraction.");
        }
        return new UnitSI(u1.Value - u2.Value, u1.Unit);
    }

    [SqlMethod(OnNullCall = false)]
    public static UnitSI Multiply(UnitSI u1, UnitSI u2)
    {
        if (u1.IsNull || u2.IsNull) return Null;

        string resultUnit;
        if (u1.Unit.Equals("none", StringComparison.OrdinalIgnoreCase) && u2.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = "none";
        }
        else if (u1.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = u2.Unit;
        }
        else if (u2.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = u1.Unit;
        }
        else
        {
            resultUnit = $"{u1.Unit}*{u2.Unit}";
        }
        return new UnitSI(u1.Value * u2.Value, resultUnit);
    }

    [SqlMethod(OnNullCall = false)]
    public static UnitSI Divide(UnitSI u1, UnitSI u2)
    {
        if (u1.IsNull || u2.IsNull) return Null;
        if (u2.Value == 0) throw new DivideByZeroException("Cannot divide by zero value UnitSI.");

        string resultUnit;
        if (u1.Unit.Equals("none", StringComparison.OrdinalIgnoreCase) && u2.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = "none";
        }
        else if (u2.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = u1.Unit;
        }
        else if (u1.Unit.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            resultUnit = $"1/{u2.Unit}";
        }
        else
        {
            resultUnit = $"{u1.Unit}/{u2.Unit}";
        }
        return new UnitSI(u1.Value / u2.Value, resultUnit);
    }

    [SqlMethod(OnNullCall = false)]
    public static UnitSI MultiplyByScalar(UnitSI u, double scalar)
    {
        if (u.IsNull) return Null;
        return new UnitSI(u.Value * scalar, u.Unit);
    }

    [SqlMethod(OnNullCall = false)]
    public static UnitSI DivideByScalar(UnitSI u, double scalar)
    {
        if (u.IsNull) return Null;
        if (scalar == 0) throw new DivideByZeroException("Cannot divide by zero scalar.");
        return new UnitSI(u.Value / scalar, u.Unit);
    }

    [SqlMethod(OnNullCall = false)]
    public SqlString ToPrefixedString(SqlString prefix)
    {
        string currentUnit = this.Unit;
        var prefixes = GetPrefixes();
        var baseSiUnits = GetBaseSiUnits();

        if (this.IsNull) return SqlString.Null;
        if (prefix.IsNull || string.IsNullOrEmpty(prefix.Value))
        {
  
            if (currentUnit.StartsWith("kg", StringComparison.OrdinalIgnoreCase))
            {
    
                double valueInGrams = this.Value * prefixes["k"]; 
                return new SqlString($"{valueInGrams.ToString(CultureInfo.InvariantCulture)} [g]");
            }
            else
            {
                return new SqlString(this.ToString());
            }
        }

        string targetPrefix = prefix.Value;
        double targetFactor;
       

        if (!prefixes.TryGetValue(targetPrefix, out targetFactor) || targetFactor == 0)
        {
            throw new ArgumentException($"Unknown or invalid prefix: '{targetPrefix}'.");
        }

        double currentValue = this.Value;
        
        string newUnitPrefix = targetPrefix;
        string baseUnitForDisplay = currentUnit; 

        string actualBaseUnitOfCurrent = currentUnit;
        double currentUnitPrefixFactor = 1.0;


        foreach (var p in prefixes.Keys)
        {
            if (p != "" && currentUnit.StartsWith(p, StringComparison.Ordinal) && currentUnit.Length > p.Length)
            {
                string tempBase = currentUnit.Substring(p.Length);

                if (baseSiUnits.Contains(tempBase))
                {
                    actualBaseUnitOfCurrent = tempBase;
                    currentUnitPrefixFactor = prefixes[p];
                    break;
                }
            }
        }

        if (currentUnit.StartsWith("kg", StringComparison.OrdinalIgnoreCase))
        {

            double valueInGrams = currentValue * prefixes["k"];
            currentValue = valueInGrams / targetFactor;
            baseUnitForDisplay = currentUnit.Substring(1);
        }
        else if (baseSiUnits.Contains(actualBaseUnitOfCurrent)) 
        {
            currentValue = (this.Value * currentUnitPrefixFactor) / targetFactor;
        }
        else
        {
            currentValue = this.Value / targetFactor;
            baseUnitForDisplay = currentUnit; 
        }

        return new SqlString($"{currentValue.ToString(CultureInfo.InvariantCulture)} [{newUnitPrefix}{baseUnitForDisplay}]");
    }

    public void Write(BinaryWriter w)
    {
        w.Write(is_Null);
        if (!is_Null)
        {
            w.Write(Value);
            w.Write(Unit ?? string.Empty);
        }
    }

    public void Read(BinaryReader r)
    {
        is_Null = r.ReadBoolean();
        if (!is_Null)
        {
            Value = r.ReadDouble();
            Unit = r.ReadString();
        }
        else
        {
            Value = 0;
            Unit = "none";
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is UnitSI))
            return false;

        UnitSI other = (UnitSI)obj;
        if (this.IsNull && other.IsNull)
            return true;
        if (this.IsNull || other.IsNull)
            return false;

        return Math.Abs(this.Value - other.Value) < 1e-9 &&
               Unit.Equals(other.Unit, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        if (this.IsNull)
            return 0;

        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Value.GetHashCode();
            hash = hash * 23 + Unit.GetHashCode();
            return hash;
        }
    }
}