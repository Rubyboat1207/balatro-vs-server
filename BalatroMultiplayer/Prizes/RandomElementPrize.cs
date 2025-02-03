namespace BalatroMultiplayer.Prizes;

public class RandomElementPrize<T>(string ident, T[] prizeOptions) : Prize
{
    public override string Identifier => ident;

    public override object? GetPrizeJson() => prizeOptions[new Random().Next(0, prizeOptions.Length)];
}