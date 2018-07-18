using System.Collections.Generic;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public interface IActivityRepository : IRepositoryBase<int, Activity>
    {
        void Start(int id);
        void Close(int id);
        void AddRunner(int activityId, int userId);
        bool IsRunnerAdded(int activityId, int userId);
        IEnumerable<Activity> GetByUserOrPublic(int userId);
        IEnumerable<Activity> GetWithAddedStatus(int userId);
    }
}
