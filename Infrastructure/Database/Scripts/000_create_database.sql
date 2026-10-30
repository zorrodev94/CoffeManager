-- Run once in SSMS using an account authorized to create databases.
USE master;
GO
IF DB_ID(N'CoffeManager') IS NULL
    CREATE DATABASE CoffeManager;
GO
