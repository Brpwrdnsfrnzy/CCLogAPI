-- Update existing users with proper email addresses and usernames
USE [CCLog];
GO

-- Update each user with an email based on their name
-- Convert name to lowercase, replace spaces with dots, and add domain

UPDATE [User]
SET 
    Email = LOWER(REPLACE(Name, ' ', '.')) + '@brisant-secure.com',
    UserName = LOWER(REPLACE(Name, ' ', '.')) + '@brisant-secure.com',
    NormalizedEmail = UPPER(LOWER(REPLACE(Name, ' ', '.')) + '@brisant-secure.com'),
    NormalizedUserName = UPPER(LOWER(REPLACE(Name, ' ', '.')) + '@brisant-secure.com'),
    EmailConfirmed = 1
WHERE UserName IS NULL;

GO

-- Display updated users for verification
SELECT ID, Name, Email, UserName, EmailConfirmed
FROM [User]
WHERE ID < 45
ORDER BY ID;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' users with email addresses and usernames.';
