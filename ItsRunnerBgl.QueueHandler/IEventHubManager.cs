using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using EventData = Microsoft.Azure.EventHubs.EventData;
using EventHubClient = Microsoft.Azure.EventHubs.EventHubClient;

namespace ItsRunnerBgl.Utility
{
    public interface IEventHubManager
    {

        Task SendMessage<T>(T data);

        Task RegisterEventProcessor<T>(string entityPath, string storageConnectionString, string storageContainerName)
            where T : IEventProcessor, new();
    }
}
