public class OrderBook
{
    public List<Order> Bids {get;} = new List<Order>();
    public List<Order> Asks {get;} = new List<Order>();

    public void AddOrder(Order order)
    {
        if (order.Side == Side.Buy) {Bids.Add(order);}
        else Asks.Add(order);
    }


    public decimal? GetBestBid()
    {
        if (Bids.Count == 0) {return null;}
        decimal best = Bids[0].Price;
        foreach (var bid in Bids)
        { if (bid.Price > best) {best = bid.Price;}}
        return best;
    }

    public decimal? GetBestAsk()
    {
        if (Asks.Count == 0) {return null;}
        decimal best = Asks[0].Price;
        foreach (var ask in Asks)
        { if (ask.Price < best) {best = ask.Price;}}
        return best;
    }

    public void RemoveOrder (Order order)
    {
        if (order.Side == Side.Buy) {Bids.Remove(order);}
        else Asks.Remove(order);
    }

    public bool CancelOrder (int OrderId)
    {
        foreach (var bid in Bids)
        if (bid.Id == OrderId)
            {
                Bids.Remove(bid);
                return true;
            }
        foreach (var ask in Asks)
        if (ask.Id == OrderId)
            {
                Asks.Remove(ask);
                return true;
            }
        return false;
    }

    public Order? GetBestAskOrder()
    {
        if (Asks.Count == 0) {return null;}
        Order best = Asks[0];
        foreach (var ask in Asks)
        { if (ask.Price < best.Price) {best = ask;}}
        return best;  
    }

    public Order? GetBestBidOrder()
    {
        if (Bids.Count == 0) {return null;}
        Order best = Bids[0];
        foreach (var bid in Bids)
        { if (bid.Price > best.Price) {best = bid;}}
        return best;  
    }
}