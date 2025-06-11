using Microsoft.SqlServer.Server; // Dla atrybutów SqlUserDefinedType, SqlProperty, SqlMethod
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;

[Serializable]
// Ustawiamy Format.UserDefined dla niestandardowej serializacji
[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = 3 * sizeof(float), ValidationMethodName = "ValidateVector3D")]
public struct Vector3D : INullable, IBinarySerialize
{
    private bool is_Null;

    // Współrzędne wektora
   
    public float x;
    public float y;
    public float z;

    // Właściwość do reprezentowania wartości NULL
    public bool IsNull
    {
        get { return is_Null; }
    }

    // Statyczna instancja NULL
    public static Vector3D Null
    {
        get
        {
            Vector3D v = new Vector3D();
            v.is_Null = true;
            return v;
        }
    }

    // Konstruktor dla łatwego tworzenia instancji
    public Vector3D(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.is_Null = false;
    }

    // Walidacja wektora (tutaj prosta, bo float zawsze jest poprawny)
    private bool ValidateVector3D()
    {
       
        return !this.IsNull;
    }

    // Metoda Parse: konwertuje string na Vector3D
    [SqlMethod(OnNullCall = false)]
    public static Vector3D Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        string value = s.Value;
        // Oczekujemy formatu: [x,y,z]
        if (!value.StartsWith("[") || !value.EndsWith("]"))
        {
            throw new ArgumentException("Invalid format for Vector3D. Expected [x,y,z].");
        }

        string[] components = value.Substring(1, value.Length - 2).Split(',');

        if (components.Length != 3)
        {
            throw new ArgumentException("Invalid number of components for Vector3D. Expected 3 (x,y,z).");
        }

        float xVal, yVal, zVal;

        // ***** KLUCZOWA ZMIANA TUTAJ: UŻYCIE CultureInfo.InvariantCulture *****
        if (!float.TryParse(components[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out xVal) ||
            !float.TryParse(components[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out yVal) ||
            !float.TryParse(components[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out zVal))
        {
            throw new ArgumentException("Invalid component type. Expected numeric values for x, y, z.");
        }

        Vector3D v = new Vector3D(xVal, yVal, zVal);

        if (!v.ValidateVector3D()) // Jeśli jest statyczna prywatna metoda walidacji
        {
            throw new ArgumentException("Invalid Vector3D values after parsing.");
        }
        return v;
    }

    // Metoda ToString: konwertuje Vector3D na string w formacie [x,y,z]
    [SqlMethod(OnNullCall = false)]
    public override string ToString()
    {
        if (is_Null)
            return "NULL";
        return $"[{x},{y},{z}]";
    }

    // Operacje arytmetyczne na wektorach (zdefiniowane jako metody SQL)

    // Dodawanie wektorów: [x1,y1,z1] + [x2,y2,z2] = [x1+x2,y1+y2,z1+z2]
    [SqlMethod(OnNullCall = false)]
    public static Vector3D Add(Vector3D v1, Vector3D v2)
    {
        if (v1.IsNull || v2.IsNull)
            return Null;

        return new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }

    // Odejmowanie wektorów: [x1,y1,z1] - [x2,y2,z2] = [x1-x2,y1-y2,z1-z2]
    [SqlMethod(OnNullCall = false)]
    public static Vector3D Subtract(Vector3D v1, Vector3D v2)
    {
        if (v1.IsNull || v2.IsNull)
            return Null;

        return new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    // Mnożenie przez skalar: [x,y,z] * scalar = [x*scalar, y*scalar, z*scalar]
    [SqlMethod(OnNullCall = false)]
    public static Vector3D MultiplyByScalar(Vector3D v, float scalar)
    {
        if (v.IsNull)
            return Null;

        return new Vector3D(v.x * scalar, v.y * scalar, v.z * scalar);
    }

    // Mnożenie skalarne (dot product): [x1,y1,z1] * [x2,y2,z2] = x1*x2 + y1*y2 + z1*z2
    [SqlMethod(OnNullCall = false)]
    public static SqlDouble DotProduct(Vector3D v1, Vector3D v2) // Zwraca SqlDouble, bo float w SQL to float w C#
    {
        if (v1.IsNull || v2.IsNull)
            return SqlDouble.Null;

        return new SqlDouble(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
    }

    // Mnożenie wektorowe (cross product):
    // [x1,y1,z1] x [x2,y2,z2] = [(y1*z2 - z1*y2), (z1*x2 - x1*z2), (x1*y2 - y1*x2)]
    [SqlMethod(OnNullCall = false)]
    public static Vector3D CrossProduct(Vector3D v1, Vector3D v2)
    {
        if (v1.IsNull || v2.IsNull)
            return Null;

        float newX = v1.y * v2.z - v1.z * v2.y;
        float newY = v1.z * v2.x - v1.x * v2.z;
        float newZ = v1.x * v2.y - v1.y * v2.x;

        return new Vector3D(newX, newY, newZ);
    }

    public void Read(BinaryReader r)
    {

        try
        {
            this.x = r.ReadSingle();
            this.y = r.ReadSingle();
            this.z = r.ReadSingle();
            this.is_Null = false;
        }
        catch (EndOfStreamException)
        {
            this.is_Null = true; // Strumień pusty, więc to jest wartość NULL
        }
    }

    public void Write(BinaryWriter w)
    {
        if (is_Null)
            return; // Nie zapisujemy nic dla NULL, jeśli IsByteOrdered jest false

        w.Write(x);
        w.Write(y);
        w.Write(z);
    }

    // Opcjonalne: Implementacja Equals i GetHashCode dla lepszej porównywalności
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Vector3D))
            return false;

        Vector3D other = (Vector3D)obj;
        if (this.IsNull && other.IsNull)
            return true;
        if (this.IsNull || other.IsNull)
            return false;

        return x == other.x && y == other.y && z == other.z;
    }

    public override int GetHashCode()
    {
        if (this.IsNull)
            return 0; // Standardowo dla null

        // Alternatywna implementacja GetHashCode() kompatybilna ze starszymi .NET Framework
        unchecked // Używamy unchecked, aby operacje arytmetyczne mogły przepełnić się bez rzucania wyjątku
        {
            int hash = 17; // Pierwsza liczba pierwsza
            hash = hash * 23 + x.GetHashCode(); // Mnożymy przez inną liczbę pierwszą i dodajemy hash x
            hash = hash * 23 + y.GetHashCode(); // Powtarzamy dla y
            hash = hash * 23 + z.GetHashCode(); // Powtarzamy dla z
            return hash;
        }
    }
}