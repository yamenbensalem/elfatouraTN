-- Run once on the CLIENT's SQL Server instance, as an admin (sysadmin / db_owner on GestCom).
--
-- Creates a dedicated, least-privilege Windows login for GestCom_Desktop instead of running
-- the app as whichever Windows account the client happens to be logged in as (often a local admin).
-- If that machine or its data is ever compromised, this account can only read/write the app's own
-- tables — it cannot alter schema, drop the database, see other databases, or do anything a
-- sysadmin/db_owner login could.
--
-- Prerequisite (run ONCE on the machine, in an elevated PowerShell/cmd — not in SSMS):
--   net user svc_gestcom "<a strong random password>" /add
--   (local account; it never interactively logs in, so Windows won't force a password change)
--
-- Then point GestCom_Desktop/appsettings.json's DefaultConnection at this account by running
-- the app as that user (e.g. via a scheduled task configured to run as svc_gestcom, or a
-- shortcut using `runas /user:.\svc_gestcom`) — Trusted_Connection stays True, so no password
-- ever appears in appsettings.json.

DECLARE @MachineName sysname = CAST(SERVERPROPERTY('MachineName') AS sysname);
DECLARE @LoginName sysname = @MachineName + N'\svc_gestcom';
DECLARE @Sql nvarchar(max);

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    SET @Sql = N'CREATE LOGIN [' + @LoginName + N'] FROM WINDOWS WITH DEFAULT_DATABASE = [GestCom];';
    EXEC (@Sql);
END

USE [GestCom];

DECLARE @UserSql nvarchar(max) = N'
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''' + @LoginName + N''')
    CREATE USER [' + @LoginName + N'] FOR LOGIN [' + @LoginName + N'];
';
EXEC (@UserSql);

-- db_datareader + db_datawriter: the app only ever does SELECT/INSERT/UPDATE/DELETE at runtime
-- (no migrations, no DDL — schema changes are applied separately by whoever manages the DB).
DECLARE @RoleSql nvarchar(max) = N'
ALTER ROLE db_datareader ADD MEMBER [' + @LoginName + N'];
ALTER ROLE db_datawriter ADD MEMBER [' + @LoginName + N'];
';
EXEC (@RoleSql);

PRINT N'Login ' + @LoginName + N' created and scoped to db_datareader/db_datawriter on GestCom.';
