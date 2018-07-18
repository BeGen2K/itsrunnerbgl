using System.Collections.Generic;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.Models.Repositories
{
    public interface ITelemetryRepository : IRepositoryBase<long, Telemetry>
    {
        IEnumerable<Telemetry> GetByActivity(int activityId);
        IEnumerable<Telemetry> GetUserByActivity(int activityId, int userId);
    }
}
