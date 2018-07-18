using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private string cs;
        public const string ApiUrlFormat = "https://localhost:44353/api/Activity/";

        public ActivityRepository(string cs)
        {
            this.cs = cs;
        }

        public IEnumerable<Activity> Get()
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Activity]
ORDER BY CreationDate DESC";

                var result = conn.Query<Activity>(query);
                return result;
            }
        }

        public IEnumerable<Activity> GetWithAddedStatus(int userId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT Activity.*, UserActivity.IdUser IsAdded
FROM [dbo].[Activity]
LEFT JOIN [dbo].[UserActivity] ON Activity.Id = UserActivity.IdActivity AND UserActivity.IdUser = @User
ORDER BY CreationDate DESC";

                var result = conn.Query<Activity>(query, new {User = userId});
                return result;
            }
        }

        public Activity Get(int id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Activity]
WHERE Id = @Id";

                var result = conn.QueryFirstOrDefault<Activity>(query, new {Id = id});
                return result;
            }
        }

        public void Update(Activity value)
        {
            throw new NotImplementedException();
        }

        public int Insert(Activity value)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
INSERT INTO [dbo].[Activity] ([Name]
           ,[IdUser]
           ,[CreationDate]
           ,[StartDate]
           ,[EndDate]
           ,[Location]
           ,[Status]
           ,[RaceUrl]
           ,[Type])
VALUES (@Name, @IdUser, @CreationDate, @StartDate, @EndDate, @Location, @Status, @RaceUrl, @Type);
SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = conn.Query<int>(query, value).Single();

                // Gare possono avere un URL (se non già specificato)
                if (value.Type == 2 && value.RaceUrl.Length == 0)
                {
                    query = @"
UPDATE [dbo].[Activity] SET [RaceUrl] = @RaceUrl WHERE [Id] = @Id";

                    var result = conn.Execute(query, new {Id = id, RaceUrl = $"{ApiUrlFormat}{id}"});

                }

                return id;
            }
        }

        

        public int GetIdFromOrganizerId(int id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT [Id] FROM [dbo].[Activity] WHERE [Type] = 2 AND [RaceUrl] = @RaceUrl";

                var result = conn.Query<int>(query, new { RaceUrl = $"{ApiUrlFormat}{id}" }).Single();
                return result;
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Start(int id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
UPDATE [dbo].[Activity] SET [Status] = 1, [StartDate] = GETDATE() WHERE [Id] = @Id AND [Status] = 0";

                var result = conn.Execute(query, new {Id = id});
            }
        }

        public void Close(int id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
UPDATE [dbo].[Activity] SET [Status] = 2, [EndDate] = GETDATE() WHERE [Id] = @Id AND [Status] = 1";

                var result = conn.Execute(query, new { Id = id });
            }
        }

        public void AddRunner(int activityId, int userId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
INSERT INTO [dbo].[UserActivity] ([IdActivity], [IdUser]) 
VALUES (@Activity, @User)";

                var result = conn.Execute(query, new { Activity = activityId, User = userId });
            }
        }

        public bool IsRunnerAdded(int activityId, int userId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT COUNT(*) FROM [dbo].[UserActivity] WHERE [IdActivity] = @Activity AND [IdUser] = @User";

                var result = conn.QueryFirstOrDefault<int>(query, new {Activity = activityId, User = userId});
                return (result > 0);
            }
        }

        public IEnumerable<Activity> GetByUserOrPublic(int userId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT Activity.*, UserActivity.IdUser IsAdded
FROM [dbo].[Activity]
LEFT JOIN [dbo].[UserActivity] ON Activity.Id = UserActivity.IdActivity AND UserActivity.IdUser = @IdUser
WHERE Activity.Type = 2 OR Activity.IdUser = @IdUser
ORDER BY CreationDate DESC";

                var result = conn.Query<Activity>(query, new {IdUser = userId});
                return result;
            }
        }
    }
}
