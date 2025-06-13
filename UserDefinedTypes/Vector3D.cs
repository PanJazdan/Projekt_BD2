using Microsoft.SqlServer.Server; // Dla atrybutów SqlUserDefinedType, SqlProperty, SqlMethod
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;

[Serializable]

[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = 3 * sizeof(float), ValidationMethodName = "ValidateVector3D")]
public struct Vector3D : INullable, IBinarySerialize
{
    private bool is_Null;


   
    public float x;
    public float y;
    public float z;

    public bool IsNull
    {
        get { return is_Null; }
    }


    public static Vector3D Null
    {
        get
        {
            Vector3D v = new Vector3D();
            v.is_Null = true;
            return v;
        }
    }

    public Vector3D(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.is_Null = false;
    }


    private bool ValidateVector3D()
    {
       
        return !this.IsNull;
    }


    [SqlMethod(OnNullCall = false)]
    public static Vector3D Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        string value = s.Value;

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


        if (!float.TryParse(components[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out xVal) ||
            !float.TryParse(components[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out yVal) ||
            !float.TryParse(components[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out zVal))
        {
            throw new ArgumentException("Invalid component type. Expected numeric values for x, y, z.");
        }

        Vector3D v = new Vector3D(xVal, yVal, zVal);

        if (!v.ValidateVector3D()) 
        {
            throw new ArgumentException("Invalid Vector3D values after parsing.");
        }
        return v;
    }


    [SqlMethod(OnNullCall = false)]
    public override string ToString()
    {
        if (is_Null)
            return "NULL";
        return $"[{x},{y},{z}]";
    }


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

   // [x1,y1,z1] * [x2,y2,z2] = x1*x2 + y1*y2 + z1*z2
    [SqlMethod(OnNullCall = false)]
    public static SqlDouble DotProduct(Vector3D v1, Vector3D v2) // Zwraca SqlDouble, bo float w SQL to float w C#
    {
        if (v1.IsNull || v2.IsNull)
            return SqlDouble.Null;

        return new SqlDouble(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
    }

   
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
            this.is_Null = true; 
        }
    }

    public void Write(BinaryWriter w)
    {
        if (is_Null)
            return; 

        w.Write(x);
        w.Write(y);
        w.Write(z);
    }

    
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
            return 0; 


        unchecked 
        {
            int hash = 17; 
            hash = hash * 23 + x.GetHashCode(); 
            hash = hash * 23 + y.GetHashCode();
            hash = hash * 23 + z.GetHashCode(); 
            return hash;
        }
    }
}