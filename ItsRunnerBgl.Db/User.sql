CREATE TABLE [dbo].[Users]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Username] NVARCHAR(50) NOT NULL, 
    [Surname] NVARCHAR(50) NOT NULL, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Birthday] DATETIME NOT NULL, 
    [Sex] SMALLINT NULL, 
    [PhotoUrl] NVARCHAR(150) NULL, 
    [IdentityUser] UNIQUEIDENTIFIER NOT NULL,
	[IsOrganizer] BIT NULL, 
)
