using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItsRunnerBgl.Models.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int IdUser { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public int Status { get; set; }
        [Required]
        public int Type { get; set; }
        public string RaceUrl { get; set; }
        public bool IsAdded { get; set; }

        [NotMapped]
        public string TypeText
        {
            get {
                switch (Type)
                {
                    case 1: return "Allenamento";
                    case 2: return "Gara";
                    default:
                        return "-";
                }
            }
        }
        [NotMapped]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    //case 1: return "Allenamento";
                    case 2: return "Chiuso";
                    case 1: return "In corso";
                    case 0: return "Aperto";
                    default: return "-";
                }
            }
        }

        
    }
}
