using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Utility
{
    public interface IQueueManager
    {

        /// <summary>
        /// Selects the queue to listen to/receive messages from.
        /// </summary>
        /// <param name="queueName">Name of the queue. All lowercase.</param>
        /// <returns>Queue reference</returns>
        Task<CloudQueue> GetQueueReference(string queueName);

        /// <summary>
        /// Sends a message to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandsQueue">Queue reference from GetQueueReference()</param>
        /// <param name="data">Object contents.</param>
        /// <returns></returns>
        Task SendMessage<T>(CloudQueue cloudQueue, T data);

        /// <summary>
        /// Receives an object from the queue.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="cloudQueue">Queue reference from GetQueueReference()</param>
        /// <returns>Deserialized object from the queue if not empty, null otherwise.</returns>
        Task<T> ReceiveMessage<T>(CloudQueue cloudQueue);
    }
}
