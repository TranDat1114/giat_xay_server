namespace giat_xay_server;

public static class OrderPriceExtensions
{
    /// <summary>
    /// Calculate the total price of the order
    /// Value: The value of the order (Kg, Quantity)
    /// </summary>
    public static decimal TotalPrice(int Value, LaundryServiceType laundryServiceType, UnitTypes unitType)
    {
        decimal totalPrice = 0;

        if (unitType == UnitTypes.Weight || unitType == UnitTypes.Time)
        {
            totalPrice = laundryServiceType.Price * (laundryServiceType.ConditionType == ConditionTypes.GreaterThan ? Value : 1);
        }
        else if (unitType == UnitTypes.Unit)
        {
            totalPrice = laundryServiceType.Price * Value;
        }

        return totalPrice;
    }

}
