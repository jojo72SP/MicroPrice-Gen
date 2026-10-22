public class Program
{
    static void Main()
    {
        // Parametres de la simulation
        int nbEvents = 10000;
        decimal startPrice = 100m;
        decimal tickSize = 0.01m;
        double cancelRate = 0.01;

        // Phase 1 : preparation
        var book = new OrderBook();
        var engine = new MatchingEngine(book);
        var history = new PriceHistory();
        int seed = Random.Shared.Next();
        Console.WriteLine($"Seed : {seed}");
        var generator = new EventGenerator(book, startPrice, tickSize, cancelRate, seed);

        // Phase 2 : la boucle
        for (int i = 0; i < nbEvents; i++)
        {
            // a. les annulations, puis un nouvel ordre
            generator.CancelOrders();
            var order = generator.NextOrder();

            // b. le moteur traite l'ordre
            var trades = engine.Submit(order);

            // c. le volume echange pendant ce tour
            int volume = 0;
            foreach (var trade in trades) { volume += trade.Quantity; }

            // d. meilleurs prix ; on saute le tour si un cote est vide
            decimal? bestBid = book.GetBestBid();
            decimal? bestAsk = book.GetBestAsk();
            if (bestBid == null || bestAsk == null) { continue; }

            // e. la photo, rangee dans l'album
            var state = new MarketState(generator.CurrentTime, volume, bestBid.Value, bestAsk.Value);
            history.Record(state);
        }

        // Phase 3 : export et mesures
        history.ExportCsv("prices.csv");
        Console.WriteLine($"Export termine : {history.MarketStates.Count} lignes");
        Console.WriteLine($"Ordres restants dans le carnet : {book.Bids.Count + book.Asks.Count}");

        decimal max = history.MarketStates[0].Mid;
        decimal min = history.MarketStates[0].Mid;
        foreach (var state in history.MarketStates)
        {
            if (state.Mid > max) { max = state.Mid; }
            if (state.Mid < min) { min = state.Mid; }
        }
        Console.WriteLine($"Amplitude du Mid : {(max - min) / tickSize} ticks");
    }
}