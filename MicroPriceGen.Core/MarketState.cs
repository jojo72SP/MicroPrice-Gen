public class MarketState
{
    public double Time {get;}
    public int Volume {get;}
    public decimal Bid {get;}
    public decimal Ask {get;}
    public decimal Mid => (Bid + Ask) / 2;
    public decimal Spread => Ask - Bid;

    public MarketState (double time, int volume, decimal bid, decimal ask)
    {
        this.Time = time;
        this.Volume = volume;
        this.Bid = bid;
        this.Ask = ask;
    }
}