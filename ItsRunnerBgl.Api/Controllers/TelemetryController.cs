using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ItsRunnerBgl.Api.Models;
using ItsRunnerBgl.Models.Models;
using ItsRunnerBgl.Models.Repositories;
using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/Telemetry")]
    public class TelemetryController : Controller
    {

        private int userId;
        private string authKey;

        //private IQueueManager _storage;

        private readonly UserManager<ApplicationUser> _userManager;
        private IQueueManager _storage;
        private IEventHubManager _eventHub;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private ITelemetryRepository _telemetryRepository;
        private IActivityRepository _activityRepository;

        public TelemetryController(ITelemetryRepository repo, IQueueManager queue, IConfiguration configuration, IUserRepository userRepository, IQueueManager storage, UserManager<ApplicationUser> userManager, IActivityRepository activityRepository, IEventHubManager eventHub)
        {
            _telemetryRepository = repo;
            //_storage = queue;
            _configuration = configuration;
            _userRepository = userRepository;
            _storage = storage;
            _userManager = userManager;
            _activityRepository = activityRepository;
            _eventHub = eventHub;
        }

        // GET: api/RunnerTelemetry

        [Authorize]
        [HttpGet("{activityId}")]
        public async Task<object> Get(int activityId, string callback)
        {
            return _telemetryRepository.GetByActivity(activityId);
        }

        // POST: api/UserActivity
        [Authorize]
        [HttpPost("{id}/Send", Name="Send")]
        public async Task<object> Create([FromRoute]int id, [FromBody]TelemetrySendViewModel model)
        {
            var activityId = id;

            if (!ModelState.IsValid)
            {
                return new {error = "Bad request."};
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userData = _userRepository.GetUserByIdentity(user.Id);

            var userId = userData.Id;

            model.IdUser = userId;
            model.IdActivity = activityId;

            // This should be changed in real code to prevent extra queries
            var activityInfo = _activityRepository.Get(model.IdActivity);
            var runnerAdded = (activityInfo.Type == 1 ||_activityRepository.IsRunnerAdded(model.IdActivity, userData.Id));
            if (userData.IsOrganizer || activityInfo.Status != 1 || !runnerAdded) // Not allowed or closed
            {
                return new { error = "Unauthorized." };
            }

            // Known format of URL
            if (activityInfo.RaceUrl.Length > 0)
            {
                var newId = 0;
                int.TryParse(activityInfo.RaceUrl.Substring(activityInfo.RaceUrl.LastIndexOf("/")+1), out newId);
                model.IdActivityOrganizer = newId;
            }

            await _eventHub.SendMessage(new QueueElement<TelemetrySendViewModel>
            {
                Type = "TelemetrySend",
                Data = model,
            });
 
            return new
            {
                coords = new
                {
                    latitude = model.Latitude,
                    longitude = model.Longitude,
                },
                imageUrl = model.Image,
            };

        }
    }
}
