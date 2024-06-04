namespace giat_xay_server;

public class Income
{
    public decimal TotalIncome { get; set; }
    public decimal TotalIncomeThisWeek { get; set; }
    public decimal TotalIncomeThisMonth { get; set; }
    public decimal TotalIncomeThisYear { get; set; }

    public int TotalOrders { get; set; }
    public int TotalOrdersThisWeek { get; set; }
    public int TotalOrdersThisMonth { get; set; }
    public int TotalOrdersThisYear { get; set; }
}
