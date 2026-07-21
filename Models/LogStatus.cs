
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class LogStatus
{
    public int ID { get; set; }

    public string Name { get; set; }


    public string Description { get; set; }

    public int RemindDays { get; set; }

    public int EscalateDays { get; set; }

    public int? OrderBy { get; set; }

    public bool CanSignOff { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}
