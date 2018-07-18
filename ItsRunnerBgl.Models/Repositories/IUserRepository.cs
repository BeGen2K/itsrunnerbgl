using System;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public interface IUserRepository : IRepositoryBase<int, User>
    {
        void UpdateImage(int userId, string imageUrl);
        int GetIdByIdentity(string identityUser);
        User GetUserByIdentity(string identityUser);
    }
}
