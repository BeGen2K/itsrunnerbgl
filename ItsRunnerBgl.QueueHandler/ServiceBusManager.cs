using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Utility
{
    public class ServiceBusManager : IServiceBusManager
    {
        private QueueClient client;

        private string queueName;

        public ServiceBusManager(string cs, string queueName)
        {
            client = new QueueClient(cs, queueName);
        }

        public async Task SendMessage<T>(T data)
        {
            var inData = JsonConvert.SerializeObject(data);
            var message = new Message(Encoding.UTF8.GetBytes(inData));
            await client.SendAsync(message);
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> function)
        {

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
            client.RegisterMessageHandler(function, messageHandlerOptions);


            /*

            client.RegisterMessageHandler(function,
                new Func<ExceptionReceivedEventArgs, Task>(async (e) =>
            {
                // error
                var x = e.ToString();
            }));
            */
        }

        public async Task CloseConnection()
        {
            await client.CloseAsync();
        }

        public async Task<T> GetMessage<T>(Message message)
        {
            // Process the message
            // Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            var msg = Encoding.UTF8.GetString(message.Body);
            var data = JsonConvert.DeserializeObject<T>(msg);

            // Complete the message so that it is not received again.
            await client.CompleteAsync(message.SystemProperties.LockToken);

            return data;
        }
    }
}
