-- Seed data for DM Auth development environment
-- Test user and test client for development purposes

-- Insert test user
-- Password: testpassword123 (BCrypt work factor 12)
INSERT INTO [Users] (
    [Id],
    [Email],
    [Username],
    [HashedPassword],
    [DisplayName],
    [IsActive],
    [EmailVerified],
    [CreatedAt]
) VALUES (
    NEWID(),
    'test@example.com',
    'testuser',
    '$2a$12$QvBFYYCsjgIyUCxRDBNoLuAfWbf7jrccRhZActh0I6hLL3Fpeu1Li',
    'Test User',
    1,
    1,
    SYSDATETIMEOFFSET()
);

-- Insert test client (public, no secret required)
INSERT INTO [Clients] (
    [Id],
    [ClientId],
    [ClientName],
    [ClientSecretHash],
    [ClientType],
    [OwnerId],
    [IsActive],
    [RedirectUris],
    [AllowedScopes],
    [CreatedAt]
) VALUES (
    NEWID(),
    'dmauth_test_client',
    'Test Client',
    NULL,
    'Public',
    (SELECT [Id] FROM [Users] WHERE [Email] = 'test@example.com'),
    1,
    '["http://localhost:5173"]',
    '["openid","profile","email","offline_access"]',
    SYSDATETIMEOFFSET()
);
