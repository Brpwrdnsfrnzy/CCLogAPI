#nullable disable

namespace CCLogAPI.Models;

/// <summary>
/// Represents a row from the SS_BRIS_CC_LU view in Sage200.
/// This view provides customer complaint lookup information.
/// </summary>
public class SageBrisCcLu
{
    public string GDN_NUMBER { get; set; }

    public short ORDER_ITEM { get; set; }

    public string STOCK_CODE { get; set; }

    public string ORDER_NUMBER { get; set; }

    public string custorder { get; set; }

    public string weborder { get; set; }

    public decimal QTY_DESPATCHED { get; set; }

    public string DateDespatch { get; set; }

    public string OrderReturnNo { get; set; }

    public string InvoiceCreditNo { get; set; }

    public string ItemDescription { get; set; }

    public string accno { get; set; }

    public string accname { get; set; }

    public string dname { get; set; }

    public string dcode { get; set; }
}
