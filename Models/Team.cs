
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class Team
{
    public int ID { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public string ViewCategories { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}