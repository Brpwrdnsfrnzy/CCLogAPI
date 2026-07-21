
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class Role
{
    public int ID { get; set; }

    public string Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}