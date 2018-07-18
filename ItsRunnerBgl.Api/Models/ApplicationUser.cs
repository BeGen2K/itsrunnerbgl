using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ItsRunnerBgl.Api.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        // public int IdRunner { get; set; }
        // public string Username { get; set; }
        /*
        public string Surname { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public int? Sex { get; set; }
        public string PhotoUrl { get; set; }*/
    }
}
