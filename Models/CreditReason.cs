
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class CreditReason
{
    public int ID { get; set; }

    public string Reason { get; set; }

    public int? OrderBy { get; set; }

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
}