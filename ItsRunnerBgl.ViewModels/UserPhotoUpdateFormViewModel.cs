using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ItsRunnerBgl.ViewModels
{
    public class UserPhotoUpdateFormViewModel
    {
        [Required]
        public int IdUser { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public IFormFile Image { get; set; }

    }
}
