-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SimDB')
BEGIN
    CREATE DATABASE SimDB;
END
GO

USE SimDB;
GO

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        Permission NVARCHAR(50)
    );
END
GO
