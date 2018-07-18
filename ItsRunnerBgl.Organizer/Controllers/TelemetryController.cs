using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItsRunnerBgl.Models.Models;
using ItsRunnerBgl.Models.Repositories;
using ItsRunnerBgl.Organizer.Models;
using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ItsRunnerBgl.Organizer.Controllers
{
    [Produces("application/json")]
    [Route("api/Telemetry")]
    public class TelemetryController : Controller
    {

        private int userId;
        private string authKey;

        //private IQueueManager _storage;

        private readonly UserManager<ApplicationUser> _userManager;
        //private IQueueManager _storage;
        //private IEventHubManager _eventHub;
        //private readonly IConfiguration _configuration;
        //private readonly IUserRepository _userRepository;
        private ITelemetryRepository _telemetryRepository;
        //private IActivityRepository _activityRepository;

        public TelemetryController(ITelemetryRepository repo, UserManager<ApplicationUser> userManager
            /*, IQueueManager queue, IConfiguration configuration,
            IUserRepository userRepository, IQueueManager storage, 
            IActivityRepository activityRepository, IEventHubManager eventHub*/)
        {
            _telemetryRepository = repo;
            _userManager = userManager;
            //_storage = queue;
            /*
            _configuration = configuration;
            _userRepository = userRepository;
            _storage = storage;
            
            _activityRepository = activityRepository;
            _eventHub = eventHub;
            */
        }

        // GET: api/RunnerTelemetry

        [Authorize]
        [HttpGet("{activityId}")]
        public async Task<object> Get(int activityId, string callback)
        {
            return _telemetryRepository.GetByActivity(activityId);
        }
        /*
        // GET: api/RunnerTelemetry/5
        [HttpGet("{activityId}/{id}", Name = "Get")]
        public string Get(int activityId, int id)
        {
            var model = JsonConvert.SerializeObject(_telemetryRepository.Get(id));
            return model;
        }*/


    }
}
