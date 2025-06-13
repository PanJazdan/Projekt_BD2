using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;

[Serializable]
[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 32, ValidationMethodName = "ValidateGeoLocation")]
public struct GeoLocation : INullable, IBinarySerialize
{
    private bool is_Null;

    public double Latitude;  // -90 to 90
    public double Longitude; // -180 to 180 

    public bool IsNull => is_Null;

    public static GeoLocation Null
    {
        get
        {
            GeoLocation g = new GeoLocation();
            g.is_Null = true;
            g.Latitude = 0;
            g.Longitude = 0;
            return g;
        }
    }

    public GeoLocation(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || double.IsInfinity(latitude) ||
            double.IsNaN(longitude) || double.IsInfinity(longitude))
            throw new ArgumentException("Latitude and Longitude must be valid numbers.");

        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be in range [-90, 90].");

        this.Latitude = latitude;
        this.Longitude = NormalizeLongitude(longitude);
        this.is_Null = false;
    }

    private static double NormalizeLongitude(double lon)
    {
        // Wrap longitude to [-180, 180]
        lon = ((lon + 180) % 360 + 360) % 360 - 180;
        return lon;
    }

    private bool ValidateGeoLocation()
    {
        if (IsNull) return true;
        return Latitude >= -90 && Latitude <= 90 &&
               !double.IsNaN(Latitude) && !double.IsInfinity(Latitude) &&
               !double.IsNaN(Longitude) && !double.IsInfinity(Longitude);
    }

    [SqlMethod(OnNullCall = false)]
    public static GeoLocation Parse(SqlString s)
    {
        if (s.IsNull) return Null;
        string value = s.Value.Trim();

      
        value = value.Trim('(', ')');
        string[] parts = value.Split(',');

        if (parts.Length != 2)
            throw new ArgumentException("Invalid format. Expected '(lat, lon)' or 'lat (N/S), lon (E/W)'.");

        double lat, lon;


        if (parts[0].Trim().EndsWith("N", StringComparison.OrdinalIgnoreCase) ||
            parts[0].Trim().EndsWith("S", StringComparison.OrdinalIgnoreCase))
        {
            lat = ParseWithDirection(parts[0].Trim(), 'N', 'S');
        }
        else
        {
            if (!double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
                throw new ArgumentException("Invalid latitude value.");
        }

        if (parts[1].Trim().EndsWith("E", StringComparison.OrdinalIgnoreCase) ||
            parts[1].Trim().EndsWith("W", StringComparison.OrdinalIgnoreCase))
        {
            lon = ParseWithDirection(parts[1].Trim(), 'E', 'W');
        }
        else
        {
            if (!double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                throw new ArgumentException("Invalid longitude value.");
        }

        return new GeoLocation(lat, lon);
    }

    private static double ParseWithDirection(string s, char pos, char neg)
    {
        s = s.ToUpperInvariant();
        double val;
        if (s.EndsWith(pos.ToString()))
        {
            if (!double.TryParse(s.Substring(0, s.Length - 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                throw new ArgumentException($"Invalid value for {pos}/{neg} coordinate.");
            return val;
        }
        else if (s.EndsWith(neg.ToString()))
        {
            if (!double.TryParse(s.Substring(0, s.Length - 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                throw new ArgumentException($"Invalid value for {pos}/{neg} coordinate.");
            return -val;
        }
        throw new ArgumentException($"Invalid direction for {pos}/{neg} coordinate.");
    }

    [SqlMethod(OnNullCall = false)]
    public override string ToString()
    {
        if (is_Null)
            return "NULL";
        // Format: (+lat, +lon)
        return $"({Latitude.ToString("+#0.######;-#0.######;0", CultureInfo.InvariantCulture)}, {Longitude.ToString("+#0.######;-#0.######;0", CultureInfo.InvariantCulture)})";
    }

    [SqlMethod(OnNullCall = false)]
    public string ToCardinalString()
    {
        if (is_Null)
            return "NULL";
        string latCard = Latitude >= 0 ? "N" : "S";
        string lonCard = Longitude >= 0 ? "E" : "W";
        return $"{Math.Abs(Latitude).ToString("0.######", CultureInfo.InvariantCulture)}{latCard}, {Math.Abs(Longitude).ToString("0.######", CultureInfo.InvariantCulture)}{lonCard}";
    }

    [SqlMethod(OnNullCall = false)]
    public static SqlDouble DistanceKm(GeoLocation g1, GeoLocation g2)
    {
        if (g1.IsNull || g2.IsNull)
            return SqlDouble.Null;

  
        double R = 6371.0; 
        double lat1 = g1.Latitude * Math.PI / 180.0;
        double lat2 = g2.Latitude * Math.PI / 180.0;
        double dLat = (g2.Latitude - g1.Latitude) * Math.PI / 180.0;
        double dLon = (g2.Longitude - g1.Longitude) * Math.PI / 180.0;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = R * c;
        return new SqlDouble(d);
    }

    public void Write(BinaryWriter w)
    {
        w.Write(is_Null);
        if (!is_Null)
        {
            w.Write(Latitude);
            w.Write(Longitude);
        }
    }

    public void Read(BinaryReader r)
    {
        is_Null = r.ReadBoolean();
        if (!is_Null)
        {
            Latitude = r.ReadDouble();
            Longitude = r.ReadDouble();
        }
        else
        {
            Latitude = 0;
            Longitude = 0;
        }
    }

    public override bool Equals(object obj)
    {
        if (!(obj is GeoLocation)) return false;
        var other = (GeoLocation)obj;
        if (this.IsNull && other.IsNull) return true;
        if (this.IsNull || other.IsNull) return false;
        return Math.Abs(this.Latitude - other.Latitude) < 1e-9 &&
               Math.Abs(this.Longitude - other.Longitude) < 1e-9;
    }

    public override int GetHashCode()
    {
        if (this.IsNull) return 0;
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Latitude.GetHashCode();
            hash = hash * 23 + Longitude.GetHashCode();
            return hash;
        }
    }
}