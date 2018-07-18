using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItsRunnerBgl.Models.Models;
using ItsRunnerBgl.Models.Repositories;
using ItsRunnerBgl.Utility;
using ItsRunnerBgl.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItsRunnerBgl.Organizer.Controllers
{
    [Produces("application/json")]
    [Route("api/Activity")]
    public class ActivityController : Controller
    {
        private IActivityRepository _activityRepository;
        private IEventHubManager _eventHub;

        public ActivityController(IActivityRepository activityRepository, IEventHubManager eventHub)
        {
            _activityRepository = activityRepository;
            _eventHub = eventHub;
        }
        /// <summary>
        ///   Called when a runner joins a shared match.
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <param name="data">Object containing the url data</param>
        /// <returns></returns>
        [HttpPost("{id}/Join", Name = "Join")]
        public async Task<Activity> Join(int id, [FromBody]ActivityStringViewModel data)
        {
            // checks would go here. oh well
            await _eventHub.SendMessage(new QueueElement<ActivityIdViewModel>
            {
                Type = "ActivityJoin",
                Data = new ActivityIdViewModel() { Id = id, IdUser = data.IdUser },
            });

            var activity = _activityRepository.Get(id);

            return activity;
        }


     
    }
}
