using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ItsRunnerBgl.Api.Services
{
    public class CommandResponsesHub : Hub
    {
        private ISignalRRegistry _registry;

        public CommandResponsesHub(ISignalRRegistry registry)
        {
            _registry = registry;
        }

        public void Register(string username)
        {
            _registry.Register(this.Context.ConnectionId, username);
        }
        /*
        // WILL NOT WORK LIKELY BECAUSE .NET CORE
        // SEE https://github.com/marcoparenzan/AdventureWorksAsync/tree/master/WebRole/Services

        private ConcurrentDictionary<string, List<string>> _connectionUsers;

        public CommandResponsesHub()
        {
            this._connectionUsers = new ConcurrentDictionary<string, List<string>>();
        }

        public async Task ImageUploadResponse(string username, string message)
        {

            if (_connectionUsers.ContainsKey(username))
            {
                var connections = _connectionUsers[username];

                await Clients.Users(connections).SendAsync("ImageUploadResponse", message);
            }
        }


        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var username = Context.User.Identity.Name;


            if (_connectionUsers.ContainsKey(username))
            {
                var connections = _connectionUsers[username];
                if (!connections.Contains(connectionId))
                    connections.Add(connectionId);
            }
            else
            {
                var value = new List<string>()
                {
                    connectionId
                };
                _connectionUsers.AddOrUpdate(username, value, (key, oldValue) => value);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var username = Context.User.Identity.Name;

            if (_connectionUsers.ContainsKey(username))
            {
                var connections = _connectionUsers[username];
                if (connections.Contains(connectionId))
                    connections.Remove(connectionId);
                if (!connections.Any())
                {
                    List<string> value;
                    _connectionUsers.TryRemove(username, out value);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        */

    }
}
