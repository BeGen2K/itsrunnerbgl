
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ItsRunnerBgl.Models.Models;
using ItsRunnerBgl.Models.Repositories;
using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Api.Controllers
{
    [Produces("application/json")]
    [Microsoft.AspNetCore.Mvc.Route("api/Activity")]
    public class ActivityController : Controller
    {
        private int userId;
        private string authKey;

        private IQueueManager _storage;
        private IConfiguration _configuration;
        private IActivityRepository _activityRepository;
        private ITelemetryRepository _telemetryRepository;


        public ActivityController(IActivityRepository activityRepository, IQueueManager queue, IConfiguration configuration, ITelemetryRepository telemetryRepository)
        {
            _activityRepository = activityRepository;
            _storage = queue;
            _configuration = configuration;
            _telemetryRepository = telemetryRepository;

            userId = 1;
            /*
            try
            {
                userId = 1; //Convert.ToInt32(Request.Cookies["user"]);
                authKey = "test"; //Request.Cookies["authKey"];
            }
            catch (NullReferenceException)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            if (userId == 0) // TEMP, should return json
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            if (authKey != UserAuth.Get(userId)) // TEMP, should return json
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }*/
        }

        // POST: api/UserActivity
        [Microsoft.AspNetCore.Mvc.HttpGet("Url/{id}", Name = "Url")]
        public async Task<Activity> GetByUrl(int id) // Activity ID
        {
            if (!Program.UseShSend || !ModelState.IsValid)
            {
                return null;
            }

            if (!_activityRepository.IsRunnerAdded(id, userId))
                _activityRepository.AddRunner(id, userId); // OTHER WORKER
            var model = _activityRepository.Get(id);

            /*
            model.IdUser = userId;
            var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
            await queue.SendMessage(new QueueElement<ActivityCreateViewModel>
            {
                Type = "ActivityCreate",
                Data = model,
            });
            */
            return model;
        }

        // :(
        [Microsoft.AspNetCore.Mvc.HttpPost("Url/{id}/Send", Name = "SendTelemetry")]
        public async Task SendTelemetry([Microsoft.AspNetCore.Mvc.FromBody] TelemetrySendViewModel model) // Activity ID
        {
            if (!Program.UseShSend || !ModelState.IsValid)
            {
                return;
            }

            var imageUrl = "";
            var telemetrySendModel = model;
            var blobStorage = new BlobManager(_configuration["StorageConnectionString"]);
            if (telemetrySendModel.Image.Length > 0)
            {
                
                var imageData = Convert.FromBase64String(FixBase64(telemetrySendModel.Image));
                imageUrl = blobStorage.UploadByteBlob(
                    blobStorage.GetContainerReference(_configuration["BlobContainerName"]),
                    $"{telemetrySendModel.IdActivity}/{telemetrySendModel.IdUser}/{DateTime.Now}_{new Random().Next(0, 20)}.png",
                    "image/png",
                    imageData
                ).GetAwaiter().GetResult();
                
            }


            _telemetryRepository.Insert(new Telemetry()
            {
                //      Id = 0,
                Longitude = telemetrySendModel.Longitude,
                Latitude = telemetrySendModel.Latitude,
                IdActivity = telemetrySendModel.IdActivity,
                IdUser = telemetrySendModel.IdUser,
                ImageUrl = imageUrl,
                Instant = telemetrySendModel.Instant
            });
            
        }

        // :(
        [Microsoft.AspNetCore.Mvc.HttpGet("Url/{id}/Telemetry", Name = "GetTelemetry")]
        public async Task<IEnumerable<Telemetry>> GetTelemetry(int id) // Activity ID
        {
            if (!Program.UseShSend || !ModelState.IsValid)
            {
                return null;
            }

            return _telemetryRepository.GetUserByActivity(id, userId);

        }

        /*
        // GET: api/UserActivity
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public IEnumerable<Activity> Get()
        {
            var data = _activityRepository.GetByUserOrPublic(userId);
            return data;
        }

        // GET: api/UserActivity/5


        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public Activity Get([FromRoute] int id)
        {
            var data = _activityRepository.Get(id);
            return data;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("Match")]
        public IEnumerable<Activity> GetMatches()
        {
            var data = _activityRepository.GetByUserOrPublic(userId).Where(x => x.Type == 2);
            return data;
        }

        // PUT: api/Activity/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}/Close", Name = "Close")]
        public async Task CloseActivity([FromRoute] int id)
        {
            var activity = _activityRepository.Get(id);
            if (activity.Status == 1)
            {
                var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
                await queue.SendMessage(new QueueElement<object>
                {
                    Type = "ActivityClose",
                    Data = new {Id = id},
                });
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{id}/Start", Name = "Start")]
        public async Task StartActivity([FromRoute] int id)
        {
            var activity = _activityRepository.Get(id);
            if (activity.Status == 0)
            {
                var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
                await queue.SendMessage(new QueueElement<object>
                {
                    Type = "ActivityStart",
                    Data = new { Id = id },
                });
            }
        }

        // POST: api/UserActivity
        [Microsoft.AspNetCore.Mvc.HttpPost("Create")]
        public async Task<IActionResult> Create([Microsoft.AspNetCore.Mvc.FromBody]ActivityCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            model.IdUser = userId;
            var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
            await queue.SendMessage(new QueueElement<ActivityCreateViewModel>
            {
                Type = "ActivityCreate",
                Data = model,
            });
            return NoContent();
        }


        //[Microsoft.AspNetCore.Mvc.HttpPost("Join")]
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}/Join", Name = "Join")]
        public async Task<IActionResult> Join(int id) //([Microsoft.AspNetCore.Mvc.FromBody]ActivityJoinViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //model.IdUser = userId;
            var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
            await queue.SendMessage(new QueueElement<ActivityJoinViewModel>
            {
                Type = "ActivityJoin",
                Data = new ActivityJoinViewModel()
                {
                    IdActivity = id,
                    IdUser = userId
                },
            });
            return NoContent();
        }*/

        public static string FixBase64(string data)
        {
            return data.Replace("&#x2B;", "+");
        }

    }
}
