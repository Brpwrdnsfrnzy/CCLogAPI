namespace CCLogAPI.Models;

public class SearchLogsRequest
{
    // Optional: search by specific log ID
    // client sends camelCase 'id'
    public int? Id { get; set; }

    // Optional: user id to filter logs by who logged or who is assigned
    // client sends camelCase 'userId'
    public int? UserId { get; set; }

    // Optional: filter by the assigned user id (matches Log.AssignedTo).
    // Some front-ends send this as 'assignedUserId'.
    public int? AssignedUserId { get; set; }

    // Optional: customer account (partial match)
    public string? CustomerAccount { get; set; }

    // Optional: customer company (partial match)
    public string? CustomerCompany { get; set; }

    // Optional: free text search against LogText
    public string? Text { get; set; }

    // Optional: status id to filter logs by their status
    public int? StatusId { get; set; }

    // Optional: allow multiple status ids to be supplied by the client
    public List<int>? StatusIds { get; set; }

    // Optional: log type id to filter logs by their type
    public int? LogTypeId { get; set; }

    // Optional: assigned team id to filter logs where the AssignedTo user's Team matches
    public int? AssignedTeamId { get; set; }

    // Paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    // Optional: filter by date range (inclusive)
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Optional: filter by completed (signed off) status. true => SignOffDate IS NOT NULL
    public bool? Completed { get; set; }

    // Optional: filter by reported issue id
    public int? ReportedIssueId { get; set; }

    // Optional: filter by sub-issue (partial match)
    public string? SubIssue { get; set; }

    // Optional: filter by return method id
    public int? ReturnMethodId { get; set; }
}
