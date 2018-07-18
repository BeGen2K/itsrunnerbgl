using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using EventData = Microsoft.Azure.EventHubs.EventData;
using EventHubClient = Microsoft.Azure.EventHubs.EventHubClient;

namespace ItsRunnerBgl.Utility
{
    public class EventHubManager : IEventHubManager
    {
        private EventHubClient client;
        private string cs;

        public EventHubManager(string cs, string entityPath)
        {
            client = EventHubClient.CreateFromConnectionString($"{cs};EntityPath={entityPath}");
            this.cs = cs;
        }

        public async Task SendMessage<T>(T data)
        {
            var inData = JsonConvert.SerializeObject(data);
            var message = new EventData(Encoding.UTF8.GetBytes(inData));
            await client.SendAsync(message);
        }
        
        public async Task RegisterEventProcessor<T>(string entityPath, string storageConnectionString, string storageContainerName)
            where T: IEventProcessor, new()
        {
            var eventProcessorHost = new EventProcessorHost(
                entityPath,
                PartitionReceiver.DefaultConsumerGroupName,
                cs,
                storageConnectionString,
                storageContainerName);

            await eventProcessorHost.RegisterEventProcessorAsync<T>();
        }
    }
}
