public class EventGenerator
{
    public OrderBook OrderBook { get; }
    public decimal StartPrice { get; }

    public decimal TickSize { get; }

    public int NextId { get; private set; } = 1;

    public double CurrentTime { get; private set; } = 0;

    public Random Tirage { get; }

    public int Seed { get; }
    public EventGenerator(OrderBook orderBook, decimal startPrice, decimal tickSize, int seed)
    {
        this.OrderBook = orderBook;
        this.StartPrice = startPrice;
        this.TickSize = tickSize;
        this.Seed = seed;
        this.Tirage = new Random(seed);
    }

    // a. faire avancer l'horloge
    // b. tirer le cote (achat ou vente)
    // c. tirer le type (limite ou marche)
    // d. tirer la quantite
    // e. calculer le prix
    // f. creer l'ordre avec le prochain Id, puis augmenter le compteur
    // g. renvoyer l'ordre

    public Order NextOrder()
    {
        CurrentTime += 1;
        Side side;
        if (Tirage.Next(2) == 0)
        {
            side = Side.Buy;
        }
        else
        {
            side = Side.Sell;
        }
        OrderType orderType;
        if (Tirage.Next(10) < 8)
        {
            orderType = OrderType.Limit;
        }
        else
        {
            orderType = OrderType.Market;
        }
        int quantity = Tirage.Next(1, 101);
        int ticks = Tirage.Next(1, 6);
        decimal price;
        if (orderType == OrderType.Market)
        {
            price = 0;
        }
        else if (side == Side.Buy)
        {
            decimal reference = OrderBook.GetBestAsk() ?? StartPrice;
            price = reference - ticks * TickSize;
        }
        else
        {
            decimal reference = OrderBook.GetBestBid() ?? StartPrice;
            price = reference + ticks * TickSize;
        }
        var order = new Order(NextId, price, quantity, CurrentTime, side, orderType);
        NextId++;  // peut également s'écrire NextId += 1; ou NextId = NextId + 1;
        return order;
    }
}