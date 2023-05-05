CREATE TABLE [dbo].[Matches] (
    Code char(32) NOT NULL PRIMARY KEY,
    StartDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    SportType nvarchar(64) NOT NULL,
    CompetitionName nvarchar(128) NOT NULL,
    HomeTeam nvarchar(128) NOT NULL,
    AwayTeam nvarchar(128) NOT NULL,
    CreatedAt datetime NOT NULL,
    UpdatedAt datetime NOT NULL,
)

