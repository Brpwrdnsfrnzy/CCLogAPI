#nullable disable
using System;
using System.Collections.Generic;

namespace CCLogAPI.Models;

public partial class Log
{
    public int ID { get; set; }

    public DateTime DateLogged { get; set; }

    public string LogText { get; set; }

    public DateTime DateLastUpdated { get; set; }

    public int LoggedBy { get; set; }

    public int LastUpdatedBy { get; set; }

    public int AssignedTo { get; set; }

    public int Status { get; set; }

    public string CustomerAccount { get; set; }

    public string CustomerContact { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerCompany { get; set; }

    public string CustomerPhone { get; set; }

    public int LogType { get; set; }

    public bool GoodsToReturn { get; set; }

    public int? GoodsReceived { get; set; }

    public int? SignOffReason { get; set; }

    public DateTime? SignOffDate { get; set; }

    public int? SignedOffBy { get; set; }

    public int? ReportedIssue { get; set; }

    public string SubIssue { get; set; }

    public int? ReturnMethod { get; set; }

    public bool? RemindedOnThisStatus { get; set; }

    public bool? EscalatedOnThisStatus { get; set; }

    public string TrackingNo { get; set; }

    public bool? LocksmithReq { get; set; }

    public decimal? LocksmithCost { get; set; }

    public decimal? LocksmithBriCost { get; set; }

    public bool? LocksmithBriToPay { get; set; }

    public bool? LocksmithOOH { get; set; }

    public bool? LocksmithCylinder { get; set; }

    public string OrderNo { get; set; }

    public string InvoiceNo { get; set; }

    public string OtherReason { get; set; }

    public bool? IsOtherReason { get; set; }

    public int? ToBlame { get; set; }

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();

    public virtual ICollection<DiaryEntry> DiaryEntries { get; set; } = new List<DiaryEntry>();

    public virtual GoodsReceivedStatus GoodsReceivedNavigation { get; set; }

    public virtual User LastUpdatedByNavigation { get; set; }

    public virtual LogType LogTypeNavigation { get; set; }

    public virtual User LoggedByNavigation { get; set; }

    public virtual ReturnMethod ReturnMethodNavigation { get; set; }

    public virtual SignOffReason SignOffReasonNavigation { get; set; }

    public virtual User SignedOffByNavigation { get; set; }

    public virtual LogStatus StatusNavigation { get; set; }
}