#nullable disable
using System;

namespace CCLogAPI.Models;

public partial class Audit
{
    public long ID { get; set; }

    public string UserName { get; set; }

    public long? LogID { get; set; }

    public string Action { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string NewStatusID { get; set; }
}
