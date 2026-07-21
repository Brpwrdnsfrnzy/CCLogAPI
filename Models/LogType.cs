
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class LogType
{
    public int ID { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}