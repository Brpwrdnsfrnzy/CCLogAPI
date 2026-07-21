using System;
using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models
{
    public class CreateLogDto
    {
        [Required]
        public string LogText { get; set; }

        [MaxLength(20)]
        public string? CustomerAccount { get; set; }

        [MaxLength(100)]
        public string? CustomerContact { get; set; }

        [MaxLength(255)]
        [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "CustomerEmail must be a valid email address or empty.")]
        public string? CustomerEmail { get; set; }

        [MaxLength(255)]
        public string? CustomerCompany { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [Required]
        public int LoggedBy { get; set; }

        [Required]
        public int LastUpdatedBy { get; set; }

        public int AssignedTo { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        public int LogType { get; set; }

        public bool GoodsToReturn { get; set; }

        // Optional FK
        public int? GoodsReceived { get; set; }

        public int? SignOffReason { get; set; }

        public DateTime? SignOffDate { get; set; }

        public int? SignedOffBy { get; set; }

        // Optional override; normally DB default will set DateLogged
        public DateTime? DateLogged { get; set; }

        // Optional FK to ReportedIssue lookup
        public int? ReportedIssue { get; set; }

        [MaxLength(40)]
        public string? SubIssue { get; set; }

        // Optional FK to ReturnMethod lookup
        public int? ReturnMethod { get; set; }

        [MaxLength(40)]
        public string? TrackingNo { get; set; }

        public bool? LocksmithReq { get; set; }

        public decimal? LocksmithCost { get; set; }

        public decimal? LocksmithBriCost { get; set; }

        public bool? LocksmithBriToPay { get; set; }

        public bool? LocksmithOOH { get; set; }

        public bool? LocksmithCylinder { get; set; }

        [MaxLength(100)]
        public string? OrderNo { get; set; }

        [MaxLength(40)]
        public string? InvoiceNo { get; set; }

        [MaxLength(255)]
        public string? OtherReason { get; set; }

        public bool? IsOtherReason { get; set; }

        public int? ToBlame { get; set; }
    }
}