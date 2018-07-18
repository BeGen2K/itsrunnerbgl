using System;

namespace ItsRunnerBgl.ViewModels
{
    public class TelemetrySendViewModel
    {
        public int IdUser { get; set; } // Optional
        public int IdActivity { get; set; } // Optional

        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTimeOffset Instant { get; set; }
        public string Image { get; set; }
        public int IdActivityOrganizer { get; set; }
    }
}
