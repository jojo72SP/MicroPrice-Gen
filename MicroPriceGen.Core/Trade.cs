public class Trade
{
    public decimal Price {get;}
    public int Quantity {get;}
    public int BuyId {get;}
    public int SellId {get;}
    public double Time {get;}

    public Trade(decimal price, int quantity, int buyId, int sellId, double time)
    {
        this.Price = price;
        this.Quantity = quantity;
        this.BuyId = buyId;
        this.SellId = sellId;
        this.Time = time;
    }
    
}