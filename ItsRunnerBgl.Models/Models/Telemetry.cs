using System;
using System.ComponentModel.DataAnnotations;
//using Microsoft.SqlServer.Types;

namespace ItsRunnerBgl.Models.Models
{
    public class Telemetry
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public int IdUser { get; set; }
        [Required]
        public int IdActivity { get; set; }
        [Required]
        public string Longitude { get; set; }
        [Required]
        public string Latitude { get; set; }
        //public SqlGeography Coordinates { get; set; }
        public DateTimeOffset Instant { get; set; }
        public string ImageUrl { get; set; }
    }
}
