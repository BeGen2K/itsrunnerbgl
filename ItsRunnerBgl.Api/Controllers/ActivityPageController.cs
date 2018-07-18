using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ItsRunnerBgl.Api.Controllers
{
    public class ActivityPageController : Controller
    {
        //private IConfiguration _configuration;
        //private ApiRequest _client;

        // private HttpClient client;
        // Temporary
        //private int userId = 1;
       // private string authKey = "test";

        private readonly UserManager<ApplicationUser> _userManager;
        private IQueueManager _storage;
        private IEventHubManager _eventHub;
        private readonly IConfiguration _configuration;
        private readonly IActivityRepository _activityRepository;
        private readonly IUserRepository _userRepository;

        public ActivityPageController(IConfiguration configuration, UserManager<ApplicationUser> userManager, IUserRepository userRepository, IActivityRepository activityRepository, IQueueManager storage, IEventHubManager eventHub)
        {
            _configuration = configuration;
            _userManager = userManager;
            _userRepository = userRepository;
            _activityRepository = activityRepository;
            _storage = storage;
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
                var activity = _activityRepository.Get(id);
                if (activity.Status == 1)
                {
                    var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
                    await queue.SendMessage(new QueueElement<object>
                    {
                        Type = "ActivityClose",
                        Data = new ActivityIdViewModel() { Id = id, IdUser = userId },
                    });
                }
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
                var activity = _activityRepository.Get(id);
                if (activity.Status == 0)
                {
                    if (Program.UseShSend)
                    {
                        _activityRepository.Start(id);
                    }
                    else if (Program.UseEventHub)
                    {
                        await _eventHub.SendMessage(new QueueElement<object>
                        {
                            Type = "ActivityStart",
                            Data = new ActivityIdViewModel() {Id = id, IdUser = userId},
                        });
                    }
                    else
                    {
                        var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"],
                            _configuration["ServiceBusQueueName"]);
                        await queue.SendMessage(new QueueElement<object>
                        {
                            Type = "ActivityStart",
                            Data = new ActivityIdViewModel() {Id = id, IdUser = userId},
                        });
                    }
                }
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
                

                if (Program.UseEventHub)
                {
                    await _eventHub.SendMessage(new QueueElement<ActivityCreateViewModel>
                    {
                        Type = "ActivityCreate",
                        Data = model,
                    });
                }
                else
                {
                    var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
                    await queue.SendMessage(new QueueElement<ActivityCreateViewModel>
                    {
                        Type = "ActivityCreate",
                        Data = model,
                    });
                }




                return RedirectToAction("Index");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }




        // GET: Activity/Create
        [Authorize]
        [HttpGet("Join")]
        public async Task<IActionResult> Join(int id)
        {
            var model = new ActivityJoinViewModel()
            {
                Activity = _activityRepository.Get(id)
            };
            return View(model);
        }

        // POST: Activity/Create
        [Authorize]
        [HttpPost("Join")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Join(ActivityJoinViewModel model)
        {
            if (ModelState.IsValid)
            {
                //await _client.GetRequest<object>($"Activity/{model.IdActivity}/Join"); // Post
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = _userRepository.GetIdByIdentity(user.Id);
                var id = model.IdActivity;
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //model.IdUser = userId;


                if (Program.UseEventHub)
                {
                    await _eventHub.SendMessage(new QueueElement<ActivityJoinViewModel>
                    {
                        Type = "ActivityJoin",
                        Data = new ActivityJoinViewModel()
                        {
                            IdActivity = id,
                            IdUser = userId
                        },
                    });
                }
                else
                {
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
                }



                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [Authorize]
        [HttpGet("JoinByUrl")]
        public async Task<IActionResult> JoinByUrl()
        {
            /* if (!Program.UseShSend)
            {
                return BadRequest();
            } */
            return View();
        }

        [Authorize]
        [HttpPost("JoinByUrl")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JoinByUrl(ActivityStringViewModel model) // contains activity id
        {
            /* if (!Program.UseShSend)
            {
                return BadRequest();
            } */
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = _userRepository.GetIdByIdentity(user.Id);
                model.IdUser = userId;

                var client = new ApiRequest($"{model.EndPointUrl}/",userId,"");
                var data = await client.PostRequest<Activity>("Join", model);

                if (data != null) // Placeholder checks
                {
                    data.IdUser = - data.IdUser;
                    data.Id = 0;
                    var newId = _activityRepository.Insert(data);
                    _activityRepository.AddRunner(newId, userId);
                }
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }
    }
}