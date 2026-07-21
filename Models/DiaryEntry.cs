using System.Text.Json.Serialization;
#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class DiaryEntry
{
    public int ID { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Text { get; set; }

    public int LogId { get; set; }

    public int? ParentId { get; set; }  // Nullable - can be NULL for root entries

    [JsonIgnore]
    public virtual ICollection<DiaryEntry> InverseParent { get; set; } = new List<DiaryEntry>();

    [JsonIgnore]
    public virtual Log Log { get; set; }

    [JsonIgnore]
    public virtual DiaryEntry Parent { get; set; }
}