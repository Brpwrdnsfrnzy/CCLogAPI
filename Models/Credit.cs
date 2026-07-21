
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class Credit
{
    public int ID { get; set; }

    public string Details { get; set; }

    public int Reason { get; set; }

    public decimal Amount { get; set; }

    public int? Log { get; set; }

    public string CreditNote { get; set; }

    public decimal? Refund { get; set; }

    public virtual Log LogNavigation { get; set; }

    public virtual CreditReason ReasonNavigation { get; set; }
}