    public class MatchingEngine
    {
        public OrderBook OrderBook {get;}

        public MatchingEngine(OrderBook book)
        {
            this.OrderBook = book;
        }
        
        public List<Trade> Submit(Order incoming)
        {
            var trades = new List<Trade>();
            while (incoming.RemainingQuantity > 0)
            {
                Order? oppositeOrder;
                if (incoming.Side == Side.Buy)
                {
                    oppositeOrder = OrderBook.GetBestAskOrder();
                }
                else {oppositeOrder = OrderBook.GetBestBidOrder();
                }
                if (oppositeOrder == null) {break;}
                if (incoming.Type == OrderType.Limit)
                {
                if (incoming.Side == Side.Buy && incoming.Price < oppositeOrder.Price) {break;}
                if (incoming.Side == Side.Sell && incoming.Price > oppositeOrder.Price) {break;}
                }
                int quantity = 0;
                quantity = Math.Min(incoming.RemainingQuantity, oppositeOrder.RemainingQuantity);
                incoming.Fill(quantity);
                oppositeOrder.Fill(quantity);
                int buyId;
                int sellId;
                if (incoming.Side == Side.Buy)
                {
                    buyId = incoming.Id;
                    sellId = oppositeOrder.Id;
                }
                else
                {
                    buyId = oppositeOrder.Id;
                    sellId = incoming.Id;
                }
                var trade = new Trade(oppositeOrder.Price, quantity, buyId, sellId, incoming.Time);
                trades.Add(trade);
                if (oppositeOrder.RemainingQuantity == 0) {OrderBook.RemoveOrder(oppositeOrder);}
            }
            if (incoming.Type == OrderType.Limit && incoming.RemainingQuantity > 0)
            {
                OrderBook.AddOrder(incoming);
            }
            return trades;
            
        }

    }