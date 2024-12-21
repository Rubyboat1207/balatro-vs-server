using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace BalatroMultiplayer.Prizes;

public abstract class Prize
{
    public static readonly Prize[] Prizes =
    [
        new RandomNumberPrize("gain_money", 5, 20),
        new RandomElementPrize<string>("random_consumable", ["Tarot", "Planet", "Spectral"]),
        new RandomJokerPrize(),
        new RandomElementPrize<CreateCardRequest>("create_joker", [
            new CreateCardRequest("ghoulish_imp", true)
        ]),
        new RandomElementPrize<CreateCardRequest>("create_consumable", [
            new CreateCardRequest("mask", true)
        ])
    ];
    public abstract string Identifier { get; }

    public abstract object? GetPrizeJson();
}

public struct CreateCardRequest(string joker, bool? modded = false)
{
    [JsonPropertyName("card")] [UsedImplicitly] public string Joker { get; set; } = joker;

    [JsonPropertyName("modded")] [UsedImplicitly] public bool? Modded { get; set; } = modded;
}