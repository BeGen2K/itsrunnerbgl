using System.ComponentModel.DataAnnotations;

namespace ItsRunnerBgl.ViewModels
{
    public class ActivityCreateViewModel
    {
        public int IdUser { get; set; }

        [Required]
        public string Name { get; set; }
      //  [Required]
      //  public int IdRunner { get; set; }
        
        //public DateTime CreationDate { get; set; }
        [Required]
        public string Location { get; set; }
       // [Required]
        public int Type { get; set; }
        //public string RaceUrl { get; set; }

    }
}
