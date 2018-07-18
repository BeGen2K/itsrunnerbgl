CREATE TABLE [dbo].[UserActivity]
(
	[IdActivity] INT NOT NULL , 
    [IdUser] INT NOT NULL, 
    CONSTRAINT [PK_UserActivity] PRIMARY KEY ([IdActivity], [IdUser])
)
