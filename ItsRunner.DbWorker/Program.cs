using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ItsRunnerBgl.Models.Models;
using ItsRunnerBgl.Models.Models.Organizer;
using ItsRunnerBgl.Models.Repositories;

using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Types;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.SqlServer.Server;

//using ExceptionReceivedEventArgs = Microsoft.Azure.ServiceBus.ExceptionReceivedEventArgs;


namespace ItsRunner.DbWorker
{
    class Program
    {
        private const bool UseEventHub = false;

        private static ActivityRepository activityRepository;
        private static UserRepository userRepository;
        private static TelemetryRepository telemetryRepository;


        //private static ServiceBusManager serviceBus;
        private static QueueClient queueClient; // Service Bus Direct
        private static ServiceBusManager responseClient;

        // Alt
       // private static EventHubManager eventHubSend;
        private static EventHubManager eventHubSendToOrganizer;

        //private static EventHubManager eventHubReceive;




        private static BlobManager blobStorage;
        private static CloudBlobContainer blobSend;
        private static CloudBlobContainer userpicSend;


        static void Main(string[] args)
        {
            // Read configuration file to get the storage connection string
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();








            /*
             * Initialize the required repositories
             */
            Console.WriteLine("Initializing repositories...");
            var sqlConnectionString = configuration["SqlConnectionString"];
            activityRepository = new ActivityRepository(sqlConnectionString);
            userRepository = new UserRepository(sqlConnectionString);
            telemetryRepository = new TelemetryRepository(sqlConnectionString);

            /*
             * Connect to the Service Bus Queues
             */
            var storageConnectionString = configuration["StorageConnectionString"];

            /*
             * BLOB Storage
             */
            blobStorage = new BlobManager(storageConnectionString);
            Console.WriteLine("Connecting to blob containers...");
            // For telemetry pictures
            var activityContainerName = configuration["BlobContainerName"];
            blobSend = blobStorage.GetContainerReference(activityContainerName);

            // For user account pictures
            var userpicContainerName = configuration["UserpicContainerName"];
            userpicSend = blobStorage.GetContainerReference(userpicContainerName);


            Console.WriteLine("Connecting to queues...");

            /*
             * Service Bus
             */

            /*
             * Service bus for sending messages back to the runner (used for redirects)
             */
            var busConnectionString = configuration["ServiceBusConnectionString"];
            var busQueueToRunnerName = configuration["ServiceBusQueueToRunnerName"];
            responseClient = new ServiceBusManager(busConnectionString, busQueueToRunnerName);

            /*
             * Service bus to receive messages from the runner
             */
            var busQueueName = configuration["ServiceBusQueueName"];
            queueClient = new QueueClient(busConnectionString, busQueueName);
            var messageHandlerOptions = new MessageHandlerOptions(new Func<Microsoft.Azure.ServiceBus.ExceptionReceivedEventArgs, Task>(
                async (e) => {
                    var x = e.ToString();
                    throw e.Exception;
                }))
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            queueClient.RegisterMessageHandler(ProcessBusMessage, messageHandlerOptions);


            /*
             * Event Hubs
             */
            var hubConnectionString = configuration["EventHubConnectionString"];

            /*
             * Event Hub to send messages to the other worker (name is a misnomer)
             */
            var hubToOrganizerEntityPath = configuration["EventHubToOrganizerEntityPath"];
            eventHubSendToOrganizer = new EventHubManager(hubConnectionString, hubToOrganizerEntityPath);

            /*
             * Event Hub to receive telemetry data (from the runner)
             * REQUIRES a table storage to keep data
             */

            var hubToWorkerContainerName = configuration["HubToWorkerContainerName"];
            var hubToWorkerEntityPath = configuration["EventHubToWorkerEntityPath"]; // Name
            var eventProcessorHost = new EventProcessorHost(
                hubToWorkerEntityPath,
                PartitionReceiver.DefaultConsumerGroupName,
                hubConnectionString,
                storageConnectionString,
                hubToWorkerContainerName);

            eventProcessorHost.RegisterEventProcessorAsync<WorkerCommandManager>().GetAwaiter().GetResult();

            // eventHubReceive.RegisterEventProcessor<WorkerCommandManager>(hubToWorkerEntityPath, connectionString,
            //    hubToWorkerContainerName).GetAwaiter().GetResult();

            while (true)
            {
                Task.Delay(1000);
            }
        }

        // Process Service Bus Messages
        public static async Task ProcessBusMessage(Message m, CancellationToken c)
        {
            var msg = Encoding.UTF8.GetString(m.Body);
            var message = JsonConvert.DeserializeObject<QueueElement<object>>(msg);
            // Complete the message so that it is not received again.
            await queueClient.CompleteAsync(m.SystemProperties.LockToken);

            await HandleMessage<object>(message);
        }

        public static T GetValue<T>(object obj)
        {
            return (T) obj.GetType().GetProperty("Items").GetValue(obj, null);
        }

        public static T ForceCast<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }

        public static string FixBase64(string data)
        {
            return data.Replace("&#x2B;", "+");
        }

        public static async Task HandleMessage<T>(QueueElement<T> message)
        {
            Console.WriteLine("Message received through HandleMessage()");
            var imageUrl = "";
            //var message = await serviceBus.GetMessage<QueueElement<object>>(m);
            int redirectId = 0;
            switch (message.Type)
            {
                case "TelemetrySend": // EVENT HUB EVENT
                    imageUrl = "";
                    var telemetrySendModel = ForceCast<TelemetrySendViewModel>(message.Data);
                    if (telemetrySendModel.Image.Length > 0)
                    {
                        var imageData = Convert.FromBase64String(FixBase64(telemetrySendModel.Image));
                        imageUrl = blobStorage.UploadByteBlob(
                            blobSend,
                            $"{telemetrySendModel.IdActivity}/{telemetrySendModel.IdUser}/{DateTime.Now}_{new Random().Next(0, 20)}.png",
                            "image/png",
                            imageData
                        ).GetAwaiter().GetResult();
                    }

                    var telemetryModel = new Telemetry()
                    {
                        Longitude = telemetrySendModel.Longitude,
                        Latitude = telemetrySendModel.Latitude,
                        IdActivity = telemetrySendModel.IdActivity,
                        IdUser = telemetrySendModel.IdUser,
                        ImageUrl = imageUrl,
                        Instant = telemetrySendModel.Instant
                    };
                    telemetryRepository.Insert(telemetryModel);
                    //--
                    telemetryModel.IdActivity = telemetrySendModel.IdActivityOrganizer;
                    await eventHubSendToOrganizer.SendMessage(new QueueElement<Telemetry>() {
                        Type = "TelemetrySend",
                        Data = telemetryModel
                    });
                    //--
                    break;
                // The organizer events cannot be closed by the client
                case "ActivityClose":
                    var closeData = ForceCast<ActivityIdViewModel>(message.Data);
                    if (closeData.FromOrganizer)
                    {
                        closeData.Id = activityRepository.GetIdFromOrganizerId(closeData.Id);
                    }

                    activityRepository.Close(closeData.Id);
                    redirectId = closeData.IdUser;
                    break;
                case "ActivityStart":
                    var startData = ForceCast<ActivityIdViewModel>(message.Data); // GetValue<int>(message.Data);
                    if (startData.FromOrganizer)
                    {
                        startData.Id = activityRepository.GetIdFromOrganizerId(startData.Id);
                    }
                    activityRepository.Start(startData.Id);
                    redirectId = startData.IdUser;
                    break;
                case "ActivityCreate":
                    var activityCreateModel = ForceCast<ActivityCreateViewModel>(message.Data);
                    activityRepository.Insert(new Activity()
                    {
                        //    Id = 0,
                        CreationDate = DateTime.Now,
                        EndDate = null,
                        Location = activityCreateModel.Location,
                        Name = activityCreateModel.Name,
                        RaceUrl = "",
                        IdUser = activityCreateModel.IdUser,
                        Status = 0,
                        Type = activityCreateModel.Type,
                    });
                    

                    redirectId = activityCreateModel.IdUser;
                    break;
                case "ActivityJoin":
                    var activityJoinModel = ForceCast<ActivityIdViewModel>(message.Data);
                    activityRepository.AddRunner(
                        activityJoinModel.Id,
                        activityJoinModel.IdUser
                    );
                    //--
                    var activityStat = activityRepository.Get(activityJoinModel.Id);
                    if (activityStat.Type == 2)
                    {
                        await eventHubSendToOrganizer.SendMessage(message);
                    }
                    //--
                    redirectId = activityJoinModel.IdUser;
                    break;
                case "UserPhotoUpdate":
                    imageUrl = "";
                    var userPhotoUpdateModel = ForceCast<UserPhotoUpdateViewModel>(message.Data);
                    if (userPhotoUpdateModel.Image.Length > 0)
                    {
                        var imageData = Convert.FromBase64String(FixBase64(userPhotoUpdateModel.Image));
                        imageUrl = blobStorage.UploadByteBlob(
                            userpicSend,
                            $"{userPhotoUpdateModel.IdUser}/{DateTime.Now}_{new Random().Next(0, 20)}.png",
                            "image/png",
                            imageData
                        ).GetAwaiter().GetResult();
                    }

                    userRepository.UpdateImage(userPhotoUpdateModel.IdUser, imageUrl);

                    redirectId = userPhotoUpdateModel.IdUser;

                    break;

            }
            if (redirectId > 0)
            {
                // Alert for redirect

                //if (UseEventHub)
                //{
                    /*
                    await eventHubSend.SendMessage(new QueueElement<SignalRRedirectViewModel>
                    {
                        Type = "SignalRRedirectResponse",
                        Data = new SignalRRedirectViewModel()
                        {
                            Username = userRepository.Get(redirectId).Name
                        },
                    });*/
                //}
               // else
               // {
                await responseClient.SendMessage(new QueueElement<SignalRRedirectViewModel>
                {
                    Type = "SignalRRedirectResponse",
                    Data = new SignalRRedirectViewModel()
                    {
                        Username = userRepository.Get(redirectId).Name
                    },
                });
                //}

            }

        }
    }

}
