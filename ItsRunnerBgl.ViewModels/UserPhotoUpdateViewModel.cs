using System;
using System.Collections.Generic;
using System.Text;

namespace ItsRunnerBgl.ViewModels
{
    public class UserPhotoUpdateViewModel
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string Image { get; set; }
        public string ContentType { get; set; }
    }
}
