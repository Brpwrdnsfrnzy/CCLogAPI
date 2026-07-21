-- Mark migrations as applied in __EFMigrationsHistory
USE [CCLog];
GO

-- Insert the migration records if they don't exist
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260224121354_ConfigureIdentityTables')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260224121354_ConfigureIdentityTables', '10.0.0');

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260224132737_AddIdentityColumnsToUser')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260224132737_AddIdentityColumnsToUser', '10.0.0');

GO

PRINT 'Migrations marked as applied!';
