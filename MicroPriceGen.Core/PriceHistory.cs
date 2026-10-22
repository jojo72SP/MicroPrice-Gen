public class PriceHistory
{
    public List<MarketState> MarketStates {get;} = new List<MarketState>();
    public void Record(MarketState marketState)
    {
        MarketStates.Add(marketState);
    }

// a. creer une liste de lignes de texte, vide
// b. ajouter la ligne d'en-tete : les noms des colonnes
// c. pour chaque photo de la liste, fabriquer une ligne et l'ajouter
// d. ecrire toutes les lignes dans le fichier
    public void ExportCsv(string path)
    {
        var lines = new List<string>();
    
        lines.Add("Time;Bid;Ask;Mid;Spread;Volume");
        foreach (var state in MarketStates)
        {
            lines.Add($"{state.Time};{state.Bid};{state.Ask};{state.Mid};{state.Spread};{state.Volume}");
        }
        File.WriteAllLines(path, lines);
    }
}