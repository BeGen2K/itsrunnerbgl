using System.Threading;
using System.Threading.Tasks;
using ItsRunnerBgl.Utility;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Organizer.Services
{
    public class OrganizerService : IHostedService
    {
        private static IHubContext<OrganizerHub> _hubStatic; // :(
        private IConfiguration _configuration;
        private IEventHubManager _eventHub;
        private IHubContext<OrganizerHub> _hub;
        private ISignalRRegistry _registry;

        public OrganizerService(IConfiguration configuration, IHubContext<OrganizerHub> hub, ISignalRRegistry registry, IEventHubManager eventHub)
        {
            _configuration = configuration;
            _hub = hub;
            _hubStatic = hub; // :(
            _registry = registry;
            _eventHub = eventHub;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            


            // Intercept messages (I hope)

            var hubConnectionString = _configuration["EventHubConnectionString"];
            var hubToWorkerEntityPath = _configuration["EventHubToWorkerEntityPath"];
            var storageConnectionString = _configuration["StorageConnectionString"];
            var hubToWorkerContainerName = _configuration["HubToWorkerContainerName"];
            var hubOrganizerPartition = _configuration["HubOrganizerPartition"];


            var eventProcessorHost = new EventProcessorHost(
                hubToWorkerEntityPath,
                hubOrganizerPartition,
                hubConnectionString,
                storageConnectionString,
                hubToWorkerContainerName);

            eventProcessorHost.RegisterEventProcessorAsync<OrganizerReceiver>().GetAwaiter().GetResult();

            // Endpoint shiz should go here
            //await _eventHub.RegisterEventProcessor<OrganizerReceiver>(hubToWorkerEntityPath, storageConnectionString,hubToWorkerContainerName);
        }

        public static async Task HandleMessage<T>(T data)
        {
           // var data = ForceCast<Telemetry>(command.Data);
            await _hubStatic.Clients.All.SendAsync("OrganizerStream", data); // :(

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public static T ForceCast<T>(object obj)
        {
            // >_>
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}
