using Microsoft.AspNetCore.SignalR;

namespace ItsRunnerBgl.Organizer.Services
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
