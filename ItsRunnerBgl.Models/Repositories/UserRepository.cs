using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public class UserRepository : IUserRepository
    {
        private string cs;

        public UserRepository(string cs)
        {
            this.cs = cs;
        }

        public IEnumerable<User> Get()
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Users]
ORDER BY Id ASC";
                var result = conn.Query<User>(query);
                return result;
            }
        }

        public User Get(int id)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Users]
WHERE Id = @Id";
                var result = conn.QueryFirstOrDefault<User>(query, new {Id = id});
                return result;
            }
        }

        public void Update(User value)
        {
            throw new NotImplementedException();
        }


        public int Insert(User value)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
INSERT INTO [dbo].[Users] (
    [Username], 
    [Surname], 
    [Name], 
    [Birthday], 
    [Sex], 
    [PhotoUrl], 
    [IdentityUser],
	[IsOrganizer])
VALUES ( 
    @Username, 
    @Surname, 
    @Name, 
    @Birthday, 
    @Sex, 
    @PhotoUrl, 
    @IdentityUser,
	@IsOrganizer)";
                return conn.Execute(query, value);
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void UpdateImage(int userId, string imageUrl)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                int result;
                // Backup old
                var query = @"
INSERT INTO [dbo].[PhotoOld] (IdUser, PhotoUrl) SELECT Id, PhotoUrl FROM [dbo].[Users] WHERE Id = @Id";
                result = conn.Execute(query, new { Id = userId});


                query = @"
UPDATE [dbo].[Users] SET PhotoUrl = @PhotoUrl WHERE Id = @Id";
                result = conn.Execute(query, new { Id = userId, PhotoUrl = imageUrl });
            }
        }

        public int GetIdByIdentity(string identityUser)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT Id FROM [dbo].[Users]
WHERE IdentityUser = @Id";
                var result = conn.QueryFirstOrDefault<User>(query, new { Id = identityUser });
                return result.Id;
            }
        }

        public User GetUserByIdentity(string identityUser)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
SELECT * FROM [dbo].[Users]
WHERE IdentityUser = @Id";
                var result = conn.QueryFirstOrDefault<User>(query, new { Id = identityUser });
                return result;
            }
        }
    }
}
