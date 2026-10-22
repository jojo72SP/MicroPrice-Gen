using System.Data;

public class Program
{
    static void Main()
    {
        // pipeline: #1: créer le carnet d'ordres - créer le moteur de matching - créer le générateur d'évènements - créer l'historique de prix
        // #2: créer les boucles

        var book = new OrderBook();
        var engine = new MatchingEngine(book);
        var history = new PriceHistory();
        int seed = Random.Shared.Next();
        Console.WriteLine($"Seed : {seed}");
        var generator = new EventGenerator(book, 100, 0.01m, seed);

        for (int i = 0; i < 1000; i++)
        {
            int volume = 0;
            if (generator.ShouldCancel())
            {
                generator.CancelRandomOrder();
            }
            else
            {
                var order = generator.NextOrder();
                var trades = engine.Submit(order);
                foreach (var trade in trades) { volume += trade.Quantity; }
            }
            decimal? bestBid = book.GetBestBid();
            decimal? bestAsk = book.GetBestAsk();
            if (bestBid == null || bestAsk == null) { continue; }
            var state = new MarketState(generator.CurrentTime, volume, bestBid.Value, bestAsk.Value);
            history.Record(state);
        }
        history.ExportCsv("prices.csv");
        Console.WriteLine($"Export termine : {history.MarketStates.Count} lignes");
        Console.WriteLine($"Ordres restants dans le carnet : {book.Bids.Count + book.Asks.Count}");
        decimal max = history.MarketStates[0].Mid;
        decimal min = history.MarketStates[0].Mid;
        foreach (var state in history.MarketStates)
        {
            if (state.Mid > max) {max = state.Mid;}
            if (state.Mid < min) {min = state.Mid;}
        }
        Console.WriteLine($"Amplitude du Mid : {(max - min) / 0.01m } ticks");



    }
}
