namespace BalatroMultiplayer.Prizes;

public abstract class Prize
{
    public static readonly Prize[] Prizes =
    [
        new RandomNumberPrize("gain_money", 5, 20),
        new RandomElementPrize<string>("random_consumable", ["Tarot", "Planet", "Spectral"]),
        new RandomJokerPrize()
    ];
    public abstract string Identifier { get; }

    public abstract object? GetPrizeJson();
}