using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItsRunnerBgl.Api.Services
{
    public class CommandResponsesService : IHostedService
    {
        /*
    private IConfiguration _configuration;
    private IHubContext<CommandResponsesHub> _hub;

    public CommandResponsesService(IConfiguration configuration, IHubContext<CommandResponsesHub> hub)
    {
        _configuration = configuration;
        _hub = hub;
    }*/

        private IConfiguration _configuration;
        private IHubContext<CommandResponsesHub> _hub;
        private ISignalRRegistry _registry;

        public CommandResponsesService(IConfiguration configuration, IHubContext<CommandResponsesHub> hub, ISignalRRegistry registry)
        {
            _configuration = configuration;
            _hub = hub;
            _registry = registry;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            var client = new QueueClient(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueToRunnerName"]);
            //var serviceBus = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueToRunnerName"]);

            var messageHandlerOptions = new MessageHandlerOptions(new Func<ExceptionReceivedEventArgs, Task>(async (e) =>
            {
                // error
                var x = e.ToString();
                throw e.Exception;
            }))
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };


            Task.Run(async () =>
            {

                client.RegisterMessageHandler(new Func<Message, CancellationToken, Task>(
                    async (Message m, CancellationToken c) => {

                        var msg = Encoding.UTF8.GetString(m.Body);
                        var command = JsonConvert.DeserializeObject<QueueElement<object>>(msg);

                        // Complete the message so that it is not received again.
                        await client.CompleteAsync(m.SystemProperties.LockToken);


                        //var command = await serviceBus.GetMessage<QueueElement<object>>(m);

                        switch (command.Type)
                        {
                            case "SignalRRedirectResponse": // doesn't need to return data
                                var data = ForceCast<SignalRRedirectViewModel>(command.Data);
                                await _hub.Clients.All.SendAsync("SignalRRedirectResponse", data);

                                //await _hub.Clients.Client(_registry.ClientIdFromUsername(data.Username)).SendAsync("ImageUploadResponse", data);
                                break;
                            default:
                                break;
                        }
                    }), messageHandlerOptions);

                while (true)
                {
                    await Task.Delay(1000);
                }
                
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken){
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
