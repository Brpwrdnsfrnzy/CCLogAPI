
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class SubIssue
{
    public int ID { get; set; }

    public string Name { get; set; }

    public int Issue { get; set; }

    public string Code { get; set; }

    public int? OrderBy { get; set; }

    public bool Deleted { get; set; }

    public virtual ReportedIssue IssueNavigation { get; set; }
}