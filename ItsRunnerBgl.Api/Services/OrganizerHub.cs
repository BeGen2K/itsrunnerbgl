using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ItsRunnerBgl.Api.Services
{
    public class OrganizerHub : Hub
    {
        private ISignalRRegistry _registry;

        public OrganizerHub(ISignalRRegistry registry)
        {
            _registry = registry;
        }

        public void Register(string username)
        {
            _registry.Register(this.Context.ConnectionId, username);
        }
    }
}
