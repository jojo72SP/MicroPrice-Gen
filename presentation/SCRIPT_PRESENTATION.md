# Script oral guidé par le code — MicroPriceGen (Joseph)

Chaque écran suit le même ordre : **cliquer sur le lien → montrer les lignes surlignées → donner l'explication juste dessous**. Les indications en gras ne se lisent pas. Garder le bloc visible pendant son explication, puis passer au suivant. Les numéros de ligne correspondent au commit `f85e2f7` (V3). Prévoir environ 20 minutes avec la démo, à mesurer en répétition.

Le fil conducteur : **on suit le chemin d'un ordre**, du générateur jusqu'au fichier CSV.

## 1. Présenter le projet

### Écran 1.1

**Montrer : la page d'accueil du dépôt, puis l'arborescence.**

**À dire :**

C'est un générateur de prix microstructurel. Au lieu de tirer un prix au hasard, on simule un carnet d'ordres : des participants envoient des ordres, un moteur les exécute, et le prix sort de ces échanges. En sortie, on obtient l'historique du Bid, de l'Ask, du Mid, du Spread et du Volume. La solution a trois projets : la logique dans Core, le programme dans App, et un projet Tests.

## 2. Les modèles de données

### Écran 2.1

**Montrer [Enums.cs, ligne 1](../MicroPriceGen.Core/Enums.cs#L1) — lignes 1 à 11.**

```csharp
public enum Side
{
    Buy,
    Sell,
}

public enum OrderType
{
    Limit,
    Market,
}
```

**À dire en pointant ce passage :**

Deux listes de valeurs fermées. Un ordre est un achat ou une vente, et un ordre limite ou un ordre au marché. Avec un enum, le compilateur refuse toute autre valeur.

### Écran 2.2

**Montrer [Order.cs, ligne 3](../MicroPriceGen.Core/Order.cs#L3) — lignes 3 à 12.**

```csharp
    public int Id { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public double Time { get; }
    
    public Side Side { get; }

    public OrderType Type { get; }

    public int RemainingQuantity { get; private set; }
```

**À dire en pointant ce passage :**

Un ordre a un numéro, un prix, une quantité, une heure, un côté et un type. Toutes ces propriétés sont en lecture seule : une fois créé, un ordre ne change pas. Le prix est en `decimal` et pas en `double`, parce qu'un `double` ne représente pas exactement 0,1 : sur des prix, les erreurs d'arrondi s'accumulent. Seule la quantité restante change, et elle est en `private set` : personne ne peut la modifier depuis l'extérieur.

### Écran 2.3

**Montrer [Order.cs, ligne 24](../MicroPriceGen.Core/Order.cs#L24) — lignes 24 à 30.**

```csharp
    public void Fill(int quantity)
    {
        if (quantity > RemainingQuantity || quantity <= 0)
        {
            throw new ArgumentOutOfRangeException();
        }
        RemainingQuantity -= quantity;
```

**À dire en pointant ce passage :**

La seule façon de modifier la quantité restante, c'est `Fill`. Elle refuse les cas impossibles : remplir plus que ce qui reste, ou une quantité nulle ou négative. Si un bug essaie de le faire, le programme s'arrête tout de suite au lieu de produire des résultats faux.

### Écran 2.4

**Montrer [Trade.cs, ligne 3](../MicroPriceGen.Core/Trade.cs#L3) — lignes 3 à 7.**

```csharp
    public decimal Price {get;}
    public int Quantity {get;}
    public int BuyId {get;}
    public int SellId {get;}
    public double Time {get;}
```

**À dire en pointant ce passage :**

Un trade est le reçu d'un échange : à quel prix, combien, entre quel acheteur et quel vendeur, et à quel moment.

### Écran 2.5

**Montrer [MarketState.cs, ligne 3](../MicroPriceGen.Core/MarketState.cs#L3) — lignes 3 à 8.**

```csharp
    public double Time {get;}
    public int Volume {get;}
    public decimal Bid {get;}
    public decimal Ask {get;}
    public decimal Mid => (Bid + Ask) / 2;
    public decimal Spread => Ask - Bid;
```

**À dire en pointant ce passage :**

C'est la photo du marché à un instant : une ligne du CSV final. Le Mid et le Spread ne sont pas stockés, ils sont calculés à partir du Bid et de l'Ask avec la flèche. Ils ne peuvent donc jamais être incohérents.

## 3. Le carnet d'ordres

### Écran 3.1

**Montrer [OrderBook.cs, ligne 3](../MicroPriceGen.Core/OrderBook.cs#L3) — lignes 3 à 10.**

```csharp
    public List<Order> Bids {get;} = new List<Order>();
    public List<Order> Asks {get;} = new List<Order>();

    public void AddOrder(Order order)
    {
        if (order.Side == Side.Buy) {Bids.Add(order);}
        else Asks.Add(order);
    }
```

**À dire en pointant ce passage :**

Le carnet, c'est la salle d'attente des ordres limites : une liste d'acheteurs et une liste de vendeurs. Un ordre est ajouté à la fin de la liste de son côté, donc les ordres sont rangés par heure d'arrivée.

### Écran 3.2

**Montrer [OrderBook.cs, ligne 13](../MicroPriceGen.Core/OrderBook.cs#L13) — lignes 13 à 20.**

```csharp
    public decimal? GetBestBid()
    {
        if (Bids.Count == 0) {return null;}
        decimal best = Bids[0].Price;
        foreach (var bid in Bids)
        { if (bid.Price > best) {best = bid.Price;}}
        return best;
    }
```

**À dire en pointant ce passage :**

Le meilleur acheteur est celui qui propose le prix le plus haut. Si personne n'attend, on renvoie `null` : c'est pour ça que le type est `decimal?`, avec un point d'interrogation. `GetBestAsk`, juste en dessous, fait la même chose avec le prix le plus bas.

### Écran 3.3

**Montrer [OrderBook.cs, ligne 54](../MicroPriceGen.Core/OrderBook.cs#L54) — lignes 54 à 61.**

```csharp
    public Order? GetBestAskOrder()
    {
        if (Asks.Count == 0) {return null;}
        Order best = Asks[0];
        foreach (var ask in Asks)
        { if (ask.Price < best.Price) {best = ask;}}
        return best;  
    }
```

**À dire en pointant ce passage :**

Celle-ci renvoie l'ordre lui-même, pas seulement son prix : le moteur en a besoin pour l'exécuter. Le point important est le `<` strict. À prix égal, on ne remplace pas : on garde le premier trouvé, donc le plus ancien, puisque les ordres sont rangés par heure d'arrivée. C'est la **priorité prix-temps** : d'abord le meilleur prix, puis le premier arrivé.

### Écran 3.4

**Montrer [OrderBook.cs, ligne 31](../MicroPriceGen.Core/OrderBook.cs#L31) — lignes 31 à 35.**

```csharp
    public void RemoveOrder (Order order)
    {
        if (order.Side == Side.Buy) {Bids.Remove(order);}
        else Asks.Remove(order);
    }
```

**À dire en pointant ce passage :**

Pour retirer un ordre du carnet : quand il est entièrement exécuté, ou quand il est annulé.

## 4. Le moteur de matching

### Écran 4.1

**Montrer [MatchingEngine.cs, ligne 5](../MicroPriceGen.Core/MatchingEngine.cs#L5) — lignes 5 à 13.**

```csharp
        public MatchingEngine(OrderBook book)
        {
            this.OrderBook = book;
        }
        
        public List<Trade> Submit(Order incoming)
        {
            var trades = new List<Trade>();
            while (incoming.RemainingQuantity > 0)
```

**À dire en pointant ce passage :**

Le moteur reçoit le carnet à sa création : il travaille sur le même carnet que le générateur. `Submit` reçoit un ordre entrant et renvoie la liste des échanges. La boucle tourne tant que l'ordre entrant n'est pas entièrement rempli, car un gros ordre peut s'exécuter contre plusieurs ordres en face.

### Écran 4.2

**Montrer [MatchingEngine.cs, ligne 15](../MicroPriceGen.Core/MatchingEngine.cs#L15) — lignes 15 à 22.**

```csharp
                Order? oppositeOrder;
                if (incoming.Side == Side.Buy)
                {
                    oppositeOrder = OrderBook.GetBestAskOrder();
                }
                else {oppositeOrder = OrderBook.GetBestBidOrder();
                }
                if (oppositeOrder == null) {break;}
```

**À dire en pointant ce passage :**

On cherche le meilleur ordre d'en face : pour un achat, le meilleur vendeur ; pour une vente, le meilleur acheteur. S'il n'y a personne, on sort de la boucle avec `break`.

### Écran 4.3

**Montrer [MatchingEngine.cs, ligne 23](../MicroPriceGen.Core/MatchingEngine.cs#L23) — lignes 23 à 27.**

```csharp
                if (incoming.Type == OrderType.Limit)
                {
                if (incoming.Side == Side.Buy && incoming.Price < oppositeOrder.Price) {break;}
                if (incoming.Side == Side.Sell && incoming.Price > oppositeOrder.Price) {break;}
                }
```

**À dire en pointant ce passage :**

Deuxième sortie : le prix ne convient pas. Un acheteur limite à 100,00 n'achète pas à 100,02. Ce contrôle ne s'applique qu'aux ordres limites : un ordre au marché accepte n'importe quel prix.

### Écran 4.4

**Montrer [MatchingEngine.cs, ligne 28](../MicroPriceGen.Core/MatchingEngine.cs#L28) — lignes 28 à 31.**

```csharp
                int quantity = 0;
                quantity = Math.Min(incoming.RemainingQuantity, oppositeOrder.RemainingQuantity);
                incoming.Fill(quantity);
                oppositeOrder.Fill(quantity);
```

**À dire en pointant ce passage :**

On échange le plus petit des deux restes. Si j'achète 100 et que le vendeur n'en a que 30, on échange 30. Puis on retire ces 30 des deux ordres avec `Fill`. C'est aussi ce qui garantit que la boucle se termine.

### Écran 4.5

**Montrer [MatchingEngine.cs, ligne 32](../MicroPriceGen.Core/MatchingEngine.cs#L32) — lignes 32 à 45.**

```csharp
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
```

**À dire en pointant ce passage :**

On enregistre l'échange. Les rôles d'acheteur et de vendeur s'inversent selon le côté de l'ordre entrant. Le prix est celui de l'**ordre au repos**, `oppositeOrder.Price` : c'est la règle des marchés. Celui qui attendait dans le carnet a fixé son prix, et celui qui arrive l'accepte.

### Écran 4.6

**Montrer [MatchingEngine.cs, ligne 46](../MicroPriceGen.Core/MatchingEngine.cs#L46) — lignes 46 à 52.**

```csharp
                if (oppositeOrder.RemainingQuantity == 0) {OrderBook.RemoveOrder(oppositeOrder);}
            }
            if (incoming.Type == OrderType.Limit && incoming.RemainingQuantity > 0)
            {
                OrderBook.AddOrder(incoming);
            }
            return trades;
```

**À dire en pointant ce passage :**

Un ordre d'en face entièrement rempli quitte le carnet. Après la boucle, s'il reste une quantité à un ordre limite, ce reliquat va attendre dans le carnet. Un ordre au marché, lui, n'attend jamais : son reste est abandonné.

## 5. Le générateur d'événements

### Écran 5.1

**Montrer [EventGenerator.cs, ligne 16](../MicroPriceGen.Core/EventGenerator.cs#L16) — lignes 16 à 24.**

```csharp
    public EventGenerator(OrderBook orderBook, decimal startPrice, decimal tickSize, double cancelRate, int seed)
    {
        this.OrderBook = orderBook;
        this.StartPrice = startPrice;
        this.TickSize = tickSize;
        this.CancelRate = cancelRate;
        this.Seed = seed;
        this.Tirage = new Random(seed);
    }
```

**À dire en pointant ce passage :**

Le générateur représente des participants « zero-intelligence » : ils agissent au hasard, mais selon des règles. Tous ses réglages viennent de l'extérieur, y compris la graine du tirage aléatoire. C'est la réponse à ta première remarque : la graine n'est plus fixe.

### Écran 5.2

**Montrer [EventGenerator.cs, ligne 29](../MicroPriceGen.Core/EventGenerator.cs#L29) — lignes 29 à 54.**

```csharp
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
```

**À dire en pointant ce passage :**

À chaque événement, l'horloge avance d'un pas. Puis on tire au hasard : achat ou vente à 50/50, ordre limite 7 fois sur 10, et une quantité entre 1 et 100. Dans `Next(1, 101)`, la borne haute est exclue, d'où le 101.

### Écran 5.3

**Montrer [EventGenerator.cs, ligne 57](../MicroPriceGen.Core/EventGenerator.cs#L57) — lignes 57 à 72.**

```csharp
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
```

**À dire en pointant ce passage :**

Le prix d'un ordre limite est placé à 1 à 5 ticks derrière le meilleur prix d'en face : un acheteur sous le meilleur vendeur, un vendeur au-dessus du meilleur acheteur. Ainsi, les ordres limites attendent dans le carnet et créent la profondeur. Les `??` disent : si ce côté est vide, on se repère sur le prix de départ. Un ordre au marché a un prix de 0, qui n'est jamais lu.

### Écran 5.4

**Montrer [EventGenerator.cs, ligne 84](../MicroPriceGen.Core/EventGenerator.cs#L84) — lignes 84 à 102.**

```csharp
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
```

**À dire en pointant ce passage :**

C'est la V3, je reviendrai sur son histoire. Chaque ordre du carnet a 1 % de chances d'être annulé à chaque tour. Plus le carnet est gros, plus il y a d'annulations : il se stabilise tout seul. Le travail se fait en deux temps, parce qu'on ne peut pas retirer un élément d'une liste pendant qu'on la parcourt avec `foreach` : le programme planterait.

## 6. L'historique et l'export

### Écran 6.1

**Montrer [PriceHistory.cs, ligne 13](../MicroPriceGen.Core/PriceHistory.cs#L13) — lignes 13 à 23.**

```csharp
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
```

**À dire en pointant ce passage :**

L'historique garde chaque photo du marché, puis l'écrit en CSV : une ligne d'en-tête, puis une ligne par photo. Le séparateur est le point-virgule, parce qu'avec un Windows français la virgule sert déjà de séparateur décimal : 100,05.

## 7. Le programme principal

### Écran 7.1

**Montrer [Program.cs, ligne 5](../MicroPriceGen.App/Program.cs#L5) — lignes 5 à 17.**

```csharp
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
```

**À dire en pointant ce passage :**

Les paramètres sont regroupés en haut : pour tester un autre réglage, on ne modifie qu'une ligne. Ensuite, on crée les objets. Un seul carnet, partagé par le moteur et le générateur. La graine est tirée au hasard et affichée à chaque lancement : on peut rejouer une simulation intéressante en remettant sa graine.

### Écran 7.2

**Montrer [Program.cs, ligne 20](../MicroPriceGen.App/Program.cs#L20) — lignes 20 à 31.**

```csharp
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
```

**À dire en pointant ce passage :**

Chaque tour de boucle est un événement de marché : des annulations, puis un nouvel ordre envoyé au moteur. Le volume du tour est la somme des quantités échangées, puisqu'un ordre peut créer plusieurs trades.

### Écran 7.3

**Montrer [Program.cs, ligne 34](../MicroPriceGen.App/Program.cs#L34) — lignes 34 à 40.**

```csharp
            decimal? bestBid = book.GetBestBid();
            decimal? bestAsk = book.GetBestAsk();
            if (bestBid == null || bestAsk == null) { continue; }

            // e. la photo, rangee dans l'album
            var state = new MarketState(generator.CurrentTime, volume, bestBid.Value, bestAsk.Value);
            history.Record(state);
```

**À dire en pointant ce passage :**

On lit le meilleur bid et le meilleur ask. Si un côté est vide, on ne peut pas calculer de Mid : `continue` saute ce tour sans arrêter la simulation. Sinon, on prend la photo et on la range.

### Écran 7.4

**Montrer [Program.cs, ligne 44](../MicroPriceGen.App/Program.cs#L44) — lignes 44 à 55.**

```csharp
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
```

**À dire en pointant ce passage :**

À la fin, on exporte, puis on affiche deux mesures : le nombre d'ordres restants dans le carnet, et l'amplitude du Mid en ticks. Ces deux lignes ont servi à diagnostiquer et corriger le modèle. C'est l'histoire suivante.

## 8. L'histoire du modèle : V1, V2, V3

### Écran 8.1

**Montrer : [l'historique des commits](https://github.com/jojo72SP/MicroPrice-Gen/commits/main).**

**À dire :**

La première version tournait, mais le prix ne bougeait presque pas : c'était ta deuxième remarque. Plutôt que de deviner, j'ai mesuré à chaque étape.

| Version | Annulations | Événements | Ordres restants | Amplitude du Mid |
| --- | --- | --- | --- | --- |
| V1 | aucune | 1 000 | 615 | 2 ticks |
| V2 | 25 % des tours, 1 ordre | 1 000 | ~37 | ~13 ticks |
| V2 | 25 % des tours, 1 ordre | 10 000 | ~450 | ~23 ticks |
| V3 | 1 % par ordre et par tour | 1 000 | ~37 | ~10,5 ticks |
| V3 | 1 % par ordre et par tour | 10 000 | ~43 | ~30 ticks |

**V1, le carnet-mur.** Sans annulation, les ordres limites s'accumulent près du prix : 615 ordres restent dans le carnet. Pour faire bouger le Mid, un ordre au marché doit vider tout un niveau de prix, et il n'y arrive jamais. Le prix reste collé à 100.

**V2, annulations à taux fixe.** Un tour sur quatre, on annule un ordre. Le prix respire sur 1 000 tours, mais sur 10 000 tours, le mur revient : on annule au plus un ordre par tour, quelle que soit la taille du carnet.

**V3, annulations proportionnelles.** Chaque ordre a 1 % de chances d'être annulé. Le carnet se stabilise autour de 40 ordres, quelle que soit la durée. Et quand on multiplie les événements par 10, l'amplitude est multipliée par 2,9, proche de racine de 10, environ 3,2 : le prix se comporte comme une marche au hasard.

## 9. Démo

### Écran 9.1

**Montrer : le terminal de VS Code.**

```
dotnet run --project MicroPriceGen.App
```

**À dire :**

Je lance deux fois de suite. La graine change à chaque lancement, donc les résultats aussi. Le carnet reste autour de 40 ordres. Puis j'ouvre le CSV dans Excel et je trace le Mid.

**Avant la démo : fermer `prices.csv` dans Excel, sinon l'export échoue.**

## 10. Conclure

### Écran 10.1

**Montrer : le graphique du Mid dans Excel.**

**À dire :**

On a suivi un ordre du générateur jusqu'au CSV : le carnet, le matching en priorité prix-temps, l'exécution au prix de l'ordre au repos, puis la photo du marché. Le modèle reste simplifié : un seul actif, une horloge à pas fixe, une volatilité qui n'est pas encore calée sur une vraie action, et pas encore de tests automatiques. Les prochaines étapes : les tests xUnit, puis caler la volatilité sur une action réelle. Sur laquelle me conseilles-tu de la caler ?
