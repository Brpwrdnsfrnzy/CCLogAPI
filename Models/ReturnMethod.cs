#nullable disable
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class ReturnMethod
{
    public int ID { get; set; }

    public string MethodName { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}
