#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CCLogAPI.Models;

public partial class User : IdentityUser<int>
{
    // Existing domain properties
    public string Name { get; set; }

    // Indicates whether the user can be assigned to logs/tasks
    public bool Assignable { get; set; }

    public int Role { get; set; }

    public int Team { get; set; }

    public virtual Role RoleNavigation { get; set; }

    public virtual Team TeamNavigation { get; set; }

    public virtual ICollection<Log> LogLastUpdatedByNavigations { get; set; } = new List<Log>();

    public virtual ICollection<Log> LogLoggedByNavigations { get; set; } = new List<Log>();

    public virtual ICollection<Log> LogSignedOffByNavigations { get; set; } = new List<Log>();
}