-- ─────────────────────────────────────────────────────────────────────────────
-- init-demo.sql — Initialisation de la base GestCom Demo
-- Utilisé par le container db-init au premier démarrage.
-- Variables substituées par sqlcmd : $(DB_NAME) $(DB_USER) $(DB_USER_PASSWORD)
-- ─────────────────────────────────────────────────────────────────────────────

USE master;
GO

-- ── 1. Créer la base si elle n'existe pas ─────────────────────────────────────
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$(DB_NAME)')
BEGIN
    CREATE DATABASE [$(DB_NAME)]
        COLLATE French_CI_AS;
    PRINT 'Base $(DB_NAME) créée.';
END
ELSE
    PRINT 'Base $(DB_NAME) déjà existante.';
GO

-- ── 2. Login SQL Server ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE LOGIN [$(DB_USER)]
        WITH PASSWORD = N'$(DB_USER_PASSWORD)',
             DEFAULT_DATABASE = [$(DB_NAME)],
             CHECK_EXPIRATION = OFF,
             CHECK_POLICY = OFF;
    PRINT 'Login $(DB_USER) créé.';
END
GO

-- ── 3. Utilisateur dans la base ───────────────────────────────────────────────
USE [$(DB_NAME)];
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'$(DB_USER)')
BEGIN
    CREATE USER [$(DB_USER)] FOR LOGIN [$(DB_USER)];
    ALTER ROLE db_owner ADD MEMBER [$(DB_USER)];
    PRINT 'Utilisateur $(DB_USER) créé avec rôle db_owner.';
END
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- NOTE : Les tables sont créées automatiquement par l'application .NET
--        au premier démarrage (Program.cs → bootstrap SQL + EF migrations).
--        Ce script ne crée que la DB et le login.
--
--        Les données de démonstration sont seedées par l'application
--        via SEED_MOCK_DATA=true (variable d'environnement dans docker-compose).
-- ─────────────────────────────────────────────────────────────────────────────
PRINT 'Initialisation terminée.';
GO
