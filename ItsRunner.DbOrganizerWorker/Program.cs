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


namespace ItsRunner.DbOrganizerWorker
{
    class Program
    {
        private static ActivityRepository activityRepository;
        private static TelemetryRepository telemetryRepository;
        private static UserRepository userRepository;

        private static ServiceBusManager queueToOtherWorker;


        static void Main(string[] args)
        {

            /*
             * This must:
             * Receive EventHub messages from configuration["EventHubToOrganizerEntityPath"]
             * Send EventHub messages from the organizer client to the other worker
             */


            // Read configuration file to get the storage connection string
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            /*
             * First of all, initialize all repositories. 
             * They all connect to the same database (here, it's the local "Organizer" database)
             */
            Console.WriteLine("Initializing repositories...");

            var sqlConnectionString = configuration["SqlConnectionString"];
            activityRepository = new ActivityRepository(sqlConnectionString);
            telemetryRepository = new TelemetryRepository(sqlConnectionString);
            userRepository = new UserRepository(sqlConnectionString);



            Console.WriteLine("Connecting to queue...");

            /*
             * Connect to the queue used to send messages to the other worker
             */
            queueToOtherWorker = new ServiceBusManager(configuration["ServiceBusConnectionString"], configuration["ServiceBusQueueName"]);

            //--



   

            /*
             * Listen to messages on the Event Hub 
             * (entity path for messages sent both by the organizer web app and the other worker)
             */
            Console.WriteLine("Listening for messages...");
            var eventProcessorHost = new EventProcessorHost(
                configuration["EventHubToOrganizerEntityPath"],
                PartitionReceiver.DefaultConsumerGroupName,
                configuration["EventHubConnectionString"],
                configuration["StorageConnectionString"],
                configuration["HubToOrganizerContainerName"]);
            eventProcessorHost.RegisterEventProcessorAsync<WorkerCommandManager>().GetAwaiter().GetResult();

            /*
             * Wait until something happened.
             */
            while (true)
            {
                Task.Delay(1000);
            }
        }

        public static T GetValue<T>(object obj)
        {
            return (T)obj.GetType().GetProperty("Items").GetValue(obj, null);
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
            switch (message.Type)
            {
                case "TelemetrySend": // FROM OTHER WORKER
                    var telemetrySendModel = ForceCast<Telemetry>(message.Data);
                    telemetryRepository.Insert(telemetrySendModel);
                    break;
                case "ActivityClose": // TO WORKER TRIO
                    var closeData = ForceCast<QueueElement<ActivityIdViewModel>>(message);
                    activityRepository.Close(closeData.Data.Id);
                    //--
                    closeData.Data.FromOrganizer = true;
                    await queueToOtherWorker.SendMessage(closeData);
                    //--
                    break;
                case "ActivityStart":
                    var startData = ForceCast<QueueElement<ActivityIdViewModel>>(message); // GetValue<int>(message.Data);
                    activityRepository.Start(startData.Data.Id);
                    //--
                    startData.Data.FromOrganizer = true;
                    await queueToOtherWorker.SendMessage(startData);
                    //--

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
                        RaceUrl = "", //$"https://localhost/raceSample/{new Random().Next(0, 1000)}",
                        IdUser = activityCreateModel.IdUser,
                        Status = 0,
                        Type = activityCreateModel.Type,
                    });
                    break;
                case "ActivityJoin":
                    var activityJoinModel = ForceCast<ActivityIdViewModel>(message.Data);
                    activityRepository.AddRunner(
                        activityJoinModel.Id,
                        activityJoinModel.IdUser
                    );
                    break;

            }

        }


    }
}
