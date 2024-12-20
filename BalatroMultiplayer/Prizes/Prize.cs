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
        new RandomElementPrize<CreateJokerRequest>("create_joker", [
            new CreateJokerRequest("ghoulish_imp", true)
        ])
    ];
    public abstract string Identifier { get; }

    public abstract object? GetPrizeJson();
}

public struct CreateJokerRequest(string joker, bool? modded = false)
{
    [JsonPropertyName("joker")] [UsedImplicitly] public string Joker { get; set; } = joker;

    [JsonPropertyName("modded")] [UsedImplicitly] public bool? Modded { get; set; } = modded;
}