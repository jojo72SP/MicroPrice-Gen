public class Order
{
    public int Id { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public double Time { get; }
    
    public Side Side { get; }

    public OrderType Type { get; }

    public int RemainingQuantity { get; private set; }
    public Order(int id, decimal price, int quantity, double time, Side side, OrderType type)
    {
        this.Id = id;
        this.Price = price;
        this.Quantity = quantity;
        this.Time = time;
        this.Side = side;
        this.Type = type;
        this.RemainingQuantity = quantity;
    }
    
    public void Fill(int quantity)
    {
        if (quantity > RemainingQuantity || quantity <= 0)
        {
            throw new ArgumentOutOfRangeException();
        }
        RemainingQuantity -= quantity;


    }

}