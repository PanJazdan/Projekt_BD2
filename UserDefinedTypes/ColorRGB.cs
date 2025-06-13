using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;


    [Serializable]
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 8, ValidationMethodName = "ValidateColorRGB")]
    public struct ColorRGB : INullable, IBinarySerialize
    {
        private bool is_Null;
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public bool IsNull => is_Null;
        public static ColorRGB Null => new ColorRGB { is_Null = true };

        public ColorRGB(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            is_Null = false;
        }

        public static ColorRGB FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex string is null or empty.");

            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                return new ColorRGB(
                    byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber)
                );
            }
            else if (hex.Length == 8)
            {
                return new ColorRGB(
                    byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber)
                );
            }
            else
            {
                throw new ArgumentException("Hex string must be 6 or 8 characters.");
            }
        }

        public string ToHex()
        {
            if (A == 255)
                return $"#{R:X2}{G:X2}{B:X2}";
            else
                return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
        }

    public ColorRGB Negate()
    {
        return new ColorRGB(
            (byte)(255 - R),
            (byte)(255 - G),
            (byte)(255 - B),
            A // preserve alpha
        );
    }

    public ColorRGB Blend(ColorRGB other, double ratio)
    {
        if (ratio < 0.0 || ratio > 1.0)
            throw new ArgumentOutOfRangeException(nameof(ratio), "Ratio must be between 0.0 and 1.0");

        byte r = (byte)Math.Round(this.R * (1.0 - ratio) + other.R * ratio);
        byte g = (byte)Math.Round(this.G * (1.0 - ratio) + other.G * ratio);
        byte b = (byte)Math.Round(this.B * (1.0 - ratio) + other.B * ratio);
        byte a = (byte)Math.Round(this.A * (1.0 - ratio) + other.A * ratio);

        return new ColorRGB(r, g, b, a);
    }

    public override string ToString()
        {
            if (IsNull) return "NULL";
            return $"[{R},{G},{B}{(A != 255 ? $",{A}" : "")}]";
        }

        public static ColorRGB Parse(SqlString s)
        {
            if (s.IsNull) return Null;
            string value = s.Value.Trim();
            if (value.StartsWith("#"))
                return FromHex(value);
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                var parts = value.Substring(1, value.Length - 2).Split(',');
                if (parts.Length == 3 || parts.Length == 4)
                {
                    byte r = byte.Parse(parts[0]);
                    byte g = byte.Parse(parts[1]);
                    byte b = byte.Parse(parts[2]);
                    byte a = (parts.Length == 4) ? byte.Parse(parts[3]) : (byte)255;
                    return new ColorRGB(r, g, b, a);
                }
            }
            throw new ArgumentException("Invalid ColorRGB format.");
        }

        private bool ValidateColorRGB()
        {
            return !IsNull;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(is_Null);
            if (!is_Null)
            {
                w.Write(R);
                w.Write(G);
                w.Write(B);
                w.Write(A);
            }
        }
        public void Read(BinaryReader r)
        {
            is_Null = r.ReadBoolean();
            if (!is_Null)
            {
                R = r.ReadByte();
                G = r.ReadByte();
                B = r.ReadByte();
                A = r.ReadByte();
            }
        }


        public override bool Equals(object obj)
        {
            if (!(obj is ColorRGB)) return false;
            var other = (ColorRGB)obj;
            return R == other.R && G == other.G && B == other.B && A == other.A && IsNull == other.IsNull;
        }

        public override int GetHashCode()
        {
            return (R << 24) | (G << 16) | (B << 8) | A;
        }
    }
