using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public class TelemetryRepository : ITelemetryRepository
    {
        private string cs;

        public TelemetryRepository(string cs)
        {
            this.cs = cs;
        }

        public IEnumerable<Telemetry> Get()
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Telemetry]
ORDER BY [Id] ASC";
                var result = conn.Query<Telemetry>(query);
                return result;
            }
        }

        public IEnumerable<Telemetry> GetByActivity(int activityId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Telemetry]
WHERE [IdActivity] = @IdActivity
ORDER BY [IdUser] ASC, [Id] ASC";
                var result = conn.Query<Telemetry>(query, new { IdActivity = activityId });
                return result;
            }
        }

        public IEnumerable<Telemetry> GetUserByActivity(int activityId, int userId)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Telemetry]
WHERE [IdActivity] = @IdActivity AND [IdUser] = @IdUser
ORDER BY [Id] ASC";
                var result = conn.Query<Telemetry>(query, new { IdActivity = activityId, IdUser = userId });
                return result;
            }
        }

        public Telemetry Get(long id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Telemetry]
WHERE [Id] = @Id";
                var result = conn.QueryFirstOrDefault<Telemetry>(query);
                return result;
            }
        }

        public void Update(Telemetry value)
        {
            throw new NotImplementedException();
        }

        public int Insert(Telemetry value)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
INSERT INTO [dbo].[Telemetry] (
    [IdUser], 
    [IdActivity], 
    [Latitude],
    [Longitude],
    [Instant], 
    [ImageUrl]
)
VALUES (@IdUser, @IdActivity, @Latitude, @Longitude, @Instant, @ImageUrl)";
                var result = conn.Execute(query, value);
                return result;
            }
        }

        public void Delete(long id)
        {
            throw new NotImplementedException();
        }
    }
}
