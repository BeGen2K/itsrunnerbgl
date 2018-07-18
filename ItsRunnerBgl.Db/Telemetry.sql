CREATE TABLE [dbo].[Telemetry]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [IdUser] INT NOT NULL, 
    [IdActivity] INT NOT NULL, 
    /* [Coordinates] [sys].[geography] NOT NULL, */
	[Latitude] TEXT NOT NULL,
	[Longitude] TEXT NOT NULL,
    [Instant] DATETIMEOFFSET NOT NULL, 
    [ImageUrl] NVARCHAR(150) NULL
)
