
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class SignOffReason
{
    public int ID { get; set; }

    public string Text { get; set; }

    public string Code { get; set; }

    public int? OrderBy { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}