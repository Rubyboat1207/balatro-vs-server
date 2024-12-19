namespace BalatroMultiplayer.Prizes;

public class RandomNumberPrize(string ident, int min, int max) : Prize
{
    public override string Identifier => ident;

    public override object GetPrizeJson() => new Random().Next(min, max);
}