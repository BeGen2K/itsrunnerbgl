using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace ItsRunnerBgl.Api.Controllers
{
    public class UserPageController : Controller
    {
      //  private IConfiguration _configuration;
       // private ApiRequest _client;

        private readonly UserManager<ApplicationUser> _userManager;
        private IQueueManager _storage;
        private IEventHubManager _eventHub;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        // private HttpClient client;
        // Temporary
        private int userId = 1;
        private string authKey = "test";

        public UserPageController(IConfiguration configuration, UserManager<ApplicationUser> userManager, IQueueManager storage, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userManager = userManager;
            _storage = storage;
            _userRepository = userRepository;
            //_client = new ApiRequest(_configuration["ApiEndpointUrl"], userId, authKey);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var model = _userRepository.Get();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = _userRepository.GetIdByIdentity(user.Id);
            ViewBag.UserId = userId;
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = _userRepository.GetIdByIdentity(user.Id);

            var model = _userRepository.Get(id);
            if (model.Id == 0 || userId != id)
            {
                return NotFound();
            }
            return View(new UserPhotoUpdateFormViewModel()
            {
                IdUser = id,
                ImageUrl = model.PhotoUrl
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserPhotoUpdateFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = _userRepository.GetIdByIdentity(user.Id);

            // Placeholder permissions check
            if (model.IdUser != userId)
            {
                return Unauthorized();
            }

            // Sanity checks
            var validUser = _userRepository.Get(model.IdUser);
            if (!ModelState.IsValid || validUser?.Id == null || validUser?.Id != userId)
            {
                return RedirectToAction("Index");
                //return BadRequest();
            }



            // Informazioni foto
            var fileExtension = Path.GetExtension(model.Image.FileName);
            // Sistema il content type
            var contentType = "";
            new FileExtensionContentTypeProvider().TryGetContentType(model.Image.FileName, out contentType);

            // Converti stream in base64
            var base64 = "";
            using (var stream = model.Image.OpenReadStream())
            {
                var outStream = new MemoryStream();
                stream.CopyTo(outStream);
                var bytes = outStream.ToArray();
                base64 = Convert.ToBase64String(bytes);
            }

            // Invia risultati

            if (Program.UseEventHub)
            {
                await _eventHub.SendMessage(new QueueElement<UserPhotoUpdateViewModel>
                {
                    Type = "UserPhotoUpdate",
                    Data = new UserPhotoUpdateViewModel()
                    {
                        ContentType = contentType,
                        IdUser = model.IdUser,
                        Image = base64,
                    },
                });
            }
            else
            {
                var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
                await queue.SendMessage(new QueueElement<UserPhotoUpdateViewModel>
                {
                    Type = "UserPhotoUpdate",
                    Data = new UserPhotoUpdateViewModel()
                    {
                        ContentType = contentType,
                        IdUser = model.IdUser,
                        Image = base64,
                    },
                });
            }




            return NoContent();
           // await _client.PostRequest<object>($"User/{model.IdUser}/UpdatePhoto", );

            //return RedirectToAction("Index");
        }
    }
}