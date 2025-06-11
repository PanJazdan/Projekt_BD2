IF TYPE_ID('Email') IS NOT NULL
    DROP TYPE Email;
IF TYPE_ID('Vector3D') IS NOT NULL
    DROP TYPE Vector3D;

IF EXISTS (SELECT * FROM sys.assemblies WHERE name = 'UDTAssembly')
    DROP ASSEMBLY UDTAssembly;

CREATE ASSEMBLY UDTAssembly
FROM 'D:\Visual Studio\Projects\Bazy danych\UserDefinedTypes\bin\Debug\UserDefinedTypes.dll'
WITH PERMISSION_SET = SAFE;
GO

CREATE TYPE Email
EXTERNAL NAME UDTAssembly.[Email];
GO

CREATE TYPE Vector3D
EXTERNAL NAME UDTAssembly.[Vector3D];