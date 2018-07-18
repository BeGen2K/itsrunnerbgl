using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ItsRunnerBgl.Utility
{
    public interface IServiceBusManager
    {
        Task SendMessage<T>(T data);
        void RegisterMessageHandler(Func<Message, CancellationToken, Task> function);
        Task CloseConnection();
        Task<T> GetMessage<T>(Message message);
    }
}
