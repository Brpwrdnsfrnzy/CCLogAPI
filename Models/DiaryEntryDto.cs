namespace CCLogAPI.Models;

public class DiaryEntryDto
{
    public int ID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LogId { get; set; }
    public int? ParentId { get; set; }
}
