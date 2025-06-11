drop table if exists Users;
drop table if exists Vectors;


CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY, 
    ContactEmail Email                  
);
GO


CREATE TABLE Vectors(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Vec Vector3D NOT NULL
    );

