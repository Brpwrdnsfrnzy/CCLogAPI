#nullable disable
using System;

namespace CCLogAPI.Models;

/// <summary>
/// Entity mapped to the SS_Bris_cust view in the Sage200_Brisant_Live_New database.
/// </summary>
public class SageBrisCustomer
{
    public string Code { get; set; }

    public string Name { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string AddressLine3 { get; set; }

    public string AddressLine4 { get; set; }

    public string City { get; set; }

    public string PostCode { get; set; }

    public string County { get; set; }

    public string Country { get; set; }

    public string AnalysisCode1 { get; set; }

    public string MainTelephoneAreaCode { get; set; }

    public string MainTelephoneSubscriberNumber { get; set; }

    public string ContactName { get; set; }

    public string Description { get; set; }

    public string ContactValue { get; set; }
}
