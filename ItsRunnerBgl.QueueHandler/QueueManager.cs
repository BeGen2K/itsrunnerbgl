using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Utility
{
    public class QueueManager : IQueueManager
    {
       // private readonly string _cs;
        private CloudQueueClient _client;

        private List<CloudQueueMessage> _messageList = new List<CloudQueueMessage>();
        /// <summary>
        /// Initializes a connection to the cloud storage account.
        /// </summary>
        /// <param name="cs">Connection string</param>
        public QueueManager(string cs)
        {
            var storageAccount = CloudStorageAccount.Parse(cs);
            _client = storageAccount.CreateCloudQueueClient();
        }

        /// <summary>
        /// Selects the queue to listen to/receive messages from.
        /// </summary>
        /// <param name="queueName">Name of the queue. All lowercase.</param>
        /// <returns>Queue reference</returns>
        public async Task<CloudQueue> GetQueueReference(string queueName)
        {
            var commandsQueue = _client.GetQueueReference(queueName);
            await commandsQueue.CreateIfNotExistsAsync();
            return commandsQueue;
        }
        /// <summary>
        /// Sends a message to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandsQueue">Queue reference from GetQueueReference()</param>
        /// <param name="data">Object contents.</param>
        /// <returns></returns>
        public async Task SendMessage<T>(CloudQueue cloudQueue, T data)
        {
            var jsonCommand = JsonConvert.SerializeObject(data);
            var message = new CloudQueueMessage(jsonCommand);
            await cloudQueue.AddMessageAsync(message);
        }

        /// <summary>
        /// Receives an object from the queue.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="cloudQueue">Queue reference from GetQueueReference()</param>
        /// <returns>Deserialized object from the queue if not empty, null otherwise.</returns>
        public async Task<T> ReceiveMessage<T>(CloudQueue cloudQueue)
        {
            var message = await cloudQueue.GetMessageAsync();
            if (message != null)
            {
                //await ;
                _messageList.Add(message);
                return JsonConvert.DeserializeObject<T>(message.AsString);
            }
            return default(T);
        }

        public async Task<string> ReceiveMessage(CloudQueue cloudQueue)
        {
            var message = await cloudQueue.GetMessageAsync();
            return message?.AsString;
        }

        public void DeleteMessages(CloudQueue cloudQueue)
        {
            foreach (CloudQueueMessage t in _messageList)
            {
                cloudQueue.DeleteMessageAsync(t);
            }
        }
    }
}
