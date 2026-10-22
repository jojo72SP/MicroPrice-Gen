public class EventGenerator
{
    public OrderBook OrderBook { get; }
    public decimal StartPrice { get; }
    public decimal TickSize { get; }

    // V3 : probabilite qu'un ordre du carnet soit annule a chaque tour (0.01 = 1 %)
    public double CancelRate { get; }

    public int Seed { get; }
    public Random Tirage { get; }

    public int NextId { get; private set; } = 1;
    public double CurrentTime { get; private set; } = 0;

    public EventGenerator(OrderBook orderBook, decimal startPrice, decimal tickSize, double cancelRate, int seed)
    {
        this.OrderBook = orderBook;
        this.StartPrice = startPrice;
        this.TickSize = tickSize;
        this.CancelRate = cancelRate;
        this.Seed = seed;
        this.Tirage = new Random(seed);
    }

    public Order NextOrder()
    {
        // a. faire avancer l'horloge
        CurrentTime += 1;

        // b. tirer le cote (achat ou vente), 50 / 50
        Side side;
        if (Tirage.Next(2) == 0)
        {
            side = Side.Buy;
        }
        else
        {
            side = Side.Sell;
        }

        // c. tirer le type : 7 chances sur 10 pour un ordre limite
        OrderType orderType;
        if (Tirage.Next(10) < 7)
        {
            orderType = OrderType.Limit;
        }
        else
        {
            orderType = OrderType.Market;
        }

        // d. tirer la quantite, entre 1 et 100
        int quantity = Tirage.Next(1, 101);

        // e. calculer le prix : 1 a 5 ticks derriere le meilleur prix d'en face
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

        // f. creer l'ordre puis augmenter le compteur
        var order = new Order(NextId, price, quantity, CurrentTime, side, orderType);
        NextId++;

        // g. renvoyer l'ordre
        return order;
    }

    // V3 : chaque ordre du carnet a CancelRate chances d'etre annule.
    // Plus le carnet est gros, plus il y a d'annulations : il se stabilise tout seul.
    public void CancelOrders()
    {
        // Temps 1 : noter les ordres a annuler, sans toucher au carnet
        var toCancel = new List<Order>();
        foreach (var order in OrderBook.Bids)
        {
            if (Tirage.NextDouble() < CancelRate) { toCancel.Add(order); }
        }
        foreach (var order in OrderBook.Asks)
        {
            if (Tirage.NextDouble() < CancelRate) { toCancel.Add(order); }
        }

        // Temps 2 : les retirer, une fois les parcours termines
        foreach (var order in toCancel)
        {
            OrderBook.RemoveOrder(order);
        }
    }
}