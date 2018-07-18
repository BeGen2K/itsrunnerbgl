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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ItsRunnerBgl.Organizer.Controllers
{
    public class ActivityPageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IEventHubManager _eventHub;
        private readonly IConfiguration _configuration;
        private readonly IActivityRepository _activityRepository;
        private readonly IUserRepository _userRepository;

        public ActivityPageController(IConfiguration configuration, UserManager<ApplicationUser> userManager, IUserRepository userRepository, IActivityRepository activityRepository, IEventHubManager eventHub)
        {
            _configuration = configuration;
            _userManager = userManager;
            _userRepository = userRepository;
            _activityRepository = activityRepository;
            _eventHub = eventHub;
            //_client = new ApiRequest(_configuration["ApiEndpointUrl"], userId, authKey);
        }

        // GET: Activity
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userData = _userRepository.GetUserByIdentity(user.Id);
            var userId = userData.Id;

            var model = _activityRepository.GetByUserOrPublic(userId);

            ViewBag.IsOrganizer = userData.IsOrganizer;
            ViewBag.IdUser = userData.Id;

            return View(model);
        }

        [Authorize]
        // GET: Activity/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var model = _activityRepository.Get(id);

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userData = _userRepository.GetUserByIdentity(user.Id);
            var userId = userData.Id;

            ViewBag.IdActivity = model.Id;
            ViewBag.IdUser = userId;
            ViewBag.IsOrganizer = userData.IsOrganizer;
            return View(model);
        }
        [Authorize]
        [HttpGet("Close")]
        public async Task<IActionResult> Close(int id)
        {
            var model = _activityRepository.Get(id);
            return View(model);
        }
        [Authorize]
        [HttpPost("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(ActivityActionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = _userRepository.GetIdByIdentity(user.Id);

                var id = model.Id;
                await _eventHub.SendMessage(new QueueElement<object>
                {
                    Type = "ActivityClose",
                    Data = new ActivityIdViewModel() { Id = id, IdUser = userId },
                });
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet("Start")]
        public async Task<IActionResult> Start(int id)
        {
            var model = _activityRepository.Get(id);
            return View(model);
        }
        [Authorize]
        [HttpPost("Start")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(ActivityActionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = _userRepository.GetIdByIdentity(user.Id);

                var id = model.Id;
                await _eventHub.SendMessage(new QueueElement<object>
                {
                    Type = "ActivityStart",
                    Data = new ActivityIdViewModel() { Id = id, IdUser = userId },
                });
            }
            return RedirectToAction("Index");
        }





        // GET: Activity/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userData = _userRepository.GetUserByIdentity(user.Id);
            ViewBag.IdUser = userData.Id;
            ViewBag.IsOrganizer = userData.IsOrganizer;
            return View();
        }

        // POST: Activity/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //await _client.PostRequest<object>($"Activity/Create", model);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userData = _userRepository.GetUserByIdentity(user.Id);
                model.IdUser = userData.Id;

                // Failsafe
                if (model.Type != 2 || !userData.IsOrganizer)
                {
                    model.Type = 1;
                }

                await _eventHub.SendMessage(new QueueElement<ActivityCreateViewModel>
                {
                    Type = "ActivityCreate",
                    Data = model,
                });

                return RedirectToAction("Index");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }




    }
}