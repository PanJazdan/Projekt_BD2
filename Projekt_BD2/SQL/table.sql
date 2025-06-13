CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY, 
    ContactEmail Email  NOT NULL              
    );

CREATE TABLE Vectors(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Vec Vector3D NOT NULL
    );

CREATE TABLE Units(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Ut UnitSI NOT NULL
    );

CREATE TABLE Locations(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Place VARCHAR(100) NOT NULL,
    Location GeoLocation NOT NULL
    );

CREATE TABLE Colors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ColorValue ColorRGB NOT NULL
);

CREATE TABLE Currency (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MoneyValue MoneyType NOT NULL
);

INSERT INTO Currency (MoneyValue)
VALUES ('123.45 [PLN]');

select * from Currency;

