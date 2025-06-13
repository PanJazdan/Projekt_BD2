IF NOT EXISTS (
    SELECT name 
    FROM sys.databases 
    WHERE name = N'BD_Projekt'
)
BEGIN
    CREATE DATABASE BD_Projekt;
END
GO


drop table if exists Users;
drop table if exists Vectors;
drop table if exists Units;
drop table if exists Locations;
drop table if exists Colors;
drop table if exists Currency;

IF TYPE_ID('Email') IS NOT NULL
    DROP TYPE Email;
IF TYPE_ID('Vector3D') IS NOT NULL
    DROP TYPE Vector3D;
IF TYPE_ID('UnitSI') IS NOT NULL
    DROP TYPE UnitSI;
IF TYPE_ID('GeoLocation') IS NOT NULL
    DROP TYPE GeoLocation;
IF TYPE_ID('ColorRGB') IS NOT NULL
    DROP TYPE ColorRGB;
IF TYPE_ID('MoneyType') IS NOT NULL
    DROP TYPE MoneyType;

IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'UDTAssembly')
    DROP ASSEMBLY UDTAssembly;


---------------------------------------------------------------------------------
--- Nale¿y wpisaæ w³asn¹ œcie¿kê lokalizacji plików, zazwyczaj bêdzie ona w strukturze
---      [POCZ¥TEK ŒCIE¯KI]\UserDefinedTypes\bin\Debug\UserDefinedTypes.dll
---------------------------------------------------------------------------------
CREATE ASSEMBLY UDTAssembly
FROM 'D:\Visual Studio\Projects\Bazy danych\UserDefinedTypes\bin\Debug\UserDefinedTypes.dll'
WITH PERMISSION_SET = SAFE;
GO
---------------------------------------------------------------------------------
---------------------------------------------------------------------------------


CREATE TYPE Email
EXTERNAL NAME UDTAssembly.[Email];

CREATE TYPE Vector3D
EXTERNAL NAME UDTAssembly.[Vector3D];

CREATE TYPE UnitSI
EXTERNAL NAME UDTAssembly.[UnitSI];

CREATE TYPE GeoLocation
EXTERNAL NAME UDTAssembly.[GeoLocation];

CREATE TYPE ColorRGB
EXTERNAL NAME UDTAssembly.[ColorRGB];

CREATE TYPE MoneyType
EXTERNAL NAME UDTAssembly.[MoneyType];

GO