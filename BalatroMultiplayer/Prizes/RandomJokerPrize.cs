﻿namespace BalatroMultiplayer.Prizes;

public class RandomJokerPrize : Prize
{
    public override string Identifier => "random_card";
    public override object GetPrizeJson() => new
    {
        rarity = new Random().Next(0, 10) == 5 ? 2 : new Random().Next(0, 2),
        card_type = "Joker"
    };
}