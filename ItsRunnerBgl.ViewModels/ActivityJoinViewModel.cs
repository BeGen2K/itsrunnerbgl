using System;
using System.Collections.Generic;
using System.Text;
using ItsRunnerBgl.Models.Models;

namespace ItsRunnerBgl.ViewModels
{
    public class ActivityJoinViewModel
    {
        public int IdUser { get; set; }
        public int IdActivity { get; set; }

        public IEnumerable<Activity> ListActivity { get; set; }
        public Activity Activity;
    }
}
