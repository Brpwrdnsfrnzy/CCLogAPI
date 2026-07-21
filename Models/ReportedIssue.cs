
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class ReportedIssue
{
    public int ID { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public int? OrderBy { get; set; }

    public int LogType { get; set; }

    public virtual ICollection<SubIssue> SubIssues { get; set; } = new List<SubIssue>();
}