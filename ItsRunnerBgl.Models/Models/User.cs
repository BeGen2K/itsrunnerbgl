using System;
using System.ComponentModel.DataAnnotations;

namespace ItsRunnerBgl.Models.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public Guid IdentityUser { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        public int? Sex { get; set; }
        public string PhotoUrl { get; set; }
  //      [Required]
   //     public bool IsRunner { get; set; }
        public bool IsOrganizer { get; set; }

    }
}
