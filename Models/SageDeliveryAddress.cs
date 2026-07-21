#nullable disable

namespace CCLogAPI.Models;

/// <summary>
/// Represents a delivery address from Sage200 SOPOrderReturn and SOPDocDelAddress tables.
/// </summary>
public class SageDeliveryAddress
{
    public string PostalName { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string AddressLine3 { get; set; }

    public string AddressLine4 { get; set; }

    public string City { get; set; }

    public string County { get; set; }

    public string Country { get; set; }

    public string Postcode { get; set; }

    public string Contact { get; set; }

    public string TelephoneNo { get; set; }

    public string EmailAddress { get; set; }
}
