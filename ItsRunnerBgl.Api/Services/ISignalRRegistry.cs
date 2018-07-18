using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItsRunnerBgl.Api.Services
{
    public interface ISignalRRegistry
    {
        void Register(string clientId, string username);
        string ClientIdFromUsername(string username);
    }
}
