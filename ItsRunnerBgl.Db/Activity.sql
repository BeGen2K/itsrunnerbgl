CREATE TABLE [dbo].[Activity]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [IdUser] INT NOT NULL, 
    [CreationDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NULL, 
	[StartDate] DATETIME NULL, 
    [Location] NVARCHAR(50) NULL, 
    [Status] SMALLINT NOT NULL, 
    [RaceUrl] NVARCHAR(50) NULL, 
    [Type] INT NOT NULL
)
