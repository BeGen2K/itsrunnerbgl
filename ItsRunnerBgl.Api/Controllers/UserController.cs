/*using System;
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

namespace ItsRunnerBgl.Api.Controllers
{
    [Produces("application/json")]
    [Microsoft.AspNetCore.Mvc.Route("api/User")]

    public class UserController : Controller
    {
        private int userId;
        private string authKey;
        private IQueueManager _storage;
        private IConfiguration _configuration;
        private IUserRepository _userRepository;

        public UserController(IQueueManager storage, IConfiguration configuration, IUserRepository userRepository)
        {
            _storage = storage;
            _configuration = configuration;
            _userRepository = userRepository;
            
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
            }
        }

        // GET: api/RunnerRunner
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public IEnumerable<User> Get()
        {
            var data = _userRepository.Get();
            return data; // new string[] { "value1", "value2" };
        }

        // GET: api/RunnerRunner/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}", Name = "Get")]
        public User Get(int id)
        {
            var data = _userRepository.Get(id);
            return data;
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("{id}/UpdatePhoto", Name = "UpdatePhoto")]
        public async Task<IActionResult> UpdatePhoto(int id, [Microsoft.AspNetCore.Mvc.FromBody] UserPhotoUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            model.IdUser = userId;
            // Placeholder permissions check
            if (model.IdUser != id)
            {
                return Unauthorized();
            }

            var queue = new ServiceBusManager(_configuration["ServiceBusConnectionString"], _configuration["ServiceBusQueueName"]);
            await queue.SendMessage(new QueueElement<UserPhotoUpdateViewModel>
            {
                Type = "UserPhotoUpdate",
                Data = model,
            });
            return NoContent();
        }
        
        
        // POST: api/RunnerRunner
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public void Post([Microsoft.AspNetCore.Mvc.FromBody]string value)
        {
        }
        
        // PUT: api/RunnerRunner/5
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public void Put(int id, [Microsoft.AspNetCore.Mvc.FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
*/