using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace BalatroMultiplayer.Prizes;

public abstract class Prize
{
    public static readonly Prize[] Prizes =
    [
        new RandomNumberPrize("gain_money", 5, 20),
        new RandomElementPrize<RandomCardRequest>("random_card", [new RandomCardRequest("Tarot"), new RandomCardRequest("Planet"), new RandomCardRequest("Spectral")]),
        new RandomJokerPrize(),
        new RandomElementPrize<CreateCardRequest>("create_card", [
            new CreateCardRequest("j_versus_ghoulish_imp", true, new {negative = true})
        ]),
        new RandomElementPrize<CreateCardRequest>("create_card", [
            new CreateCardRequest("c_versus_square", false)
        ])
    ];
    public abstract string Identifier { get; }

    public abstract object? GetPrizeJson();
}

public struct CreateCardRequest(string joker, bool? perishable, object? edition=null)
{
    [JsonPropertyName("card")] [UsedImplicitly] public string Joker { get; set; } = joker;

    [JsonPropertyName("perishable")] [UsedImplicitly] public bool? Perishable { get; set; } = perishable;

    [JsonPropertyName("edition")]
    [UsedImplicitly]
    public object? Edition { get; set; } = edition;
}

public struct RandomCardRequest(string cardType)
{
    [JsonPropertyName("card_type")] [UsedImplicitly] public string CardType { get; set; } = cardType;
}