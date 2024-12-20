using System.Text.Json;
using System.Text.Json.Serialization;
using BalatroMultiplayer.Jokers;
using JetBrains.Annotations;

namespace BalatroMultiplayer;

public abstract class InboundMessage
{
    public MessageContainer? Parent { get; set; }
    
    public virtual async Task Handle(Player[] clients, Player sender)
    {
        foreach (var client in clients.Where(c => c != sender))
        {
            if (Parent is not null)
            {
                await client.SendMessage(Parent);
            }
        }
    }
}

public class MessageContainer
{
    public MessageContainer(string? type, object? data)
    {
        Type = type;
        Data = JsonSerializer.Serialize(data);
    }
    public MessageContainer() { }

    [JsonPropertyName("type")] [UsedImplicitly] public string? Type { get; set; }

    [JsonPropertyName("data")] [UsedImplicitly] public string? Data { get; set; }

    public InboundMessage? GetHandler()
    {
        if (Data == null)
        {
            throw new Exception("Attempted to get handler on empty data");
        }
        
        InboundMessage? handler = Type switch
        {
            "update_score" => JsonSerializer.Deserialize<UpdateScoreMessage>(Data),
            "start_game" => JsonSerializer.Deserialize<StartGameMessage>(Data),
            "blind_cleared" => JsonSerializer.Deserialize<BlindClearedMessage>(Data),
            _ => null
        };

        if (handler != null)
        {
            handler.Parent = this;
        }
        
        return handler;
    }
}

public class UpdateScoreMessage : InboundMessage
{
    [JsonPropertyName("score")]
    public int Score { get; init; }
    [JsonPropertyName("blind")]
    public int Blind { get; init; }
    public override async Task Handle(Player[] clients, Player sender)
    {
        await base.Handle(clients, sender);
        BlindData.UpdateScore(sender, Blind, Score);
    }
}

public class BlindClearedMessage : InboundMessage
{
    [JsonPropertyName("blind")] public int Blind { get; init; }
    [JsonPropertyName("game_over")] public bool GameOver { get; init; }
    public override async Task Handle(Player[] clients, Player sender)
    {
        if (GameOver)
        {
            sender.LostGame = true;
        }
        BlindData.MarkCompletedFor(sender, Blind);
    }
}

public class StartGameMessage : InboundMessage
{
    
    [JsonPropertyName("seed")]
    public string? Seed { get; init; }
    [JsonPropertyName("stake")]
    public int Stake { get; init; }
    [JsonPropertyName("deck")]
    public string? Deck { get; init; }

    public override async Task Handle(Player[] clients, Player sender)
    {
        if (Program.CurrentGame is null)
        {
            await base.Handle(clients, sender);
            Program.CurrentGame = this;
        }
    }
}

public class MultiplayerJokerAbility : InboundMessage
{
    [JsonPropertyName("joker")]
    public string? Joker { get; init; }
    [JsonPropertyName("extra_data")]
    public string? ExtraData { get; init; }

    public override async Task Handle(Player[] clients, Player sender)
    {
        var handler = JokerHandler.Handlers.FirstOrDefault(jh => jh.Identifier == Joker)?.Handle(sender, ExtraData);

        if (handler is null) return;
        await handler;
    }
}

public class WinLoseMessage(bool won, string prizeType, string prizeValue, int blind)
{
    [JsonPropertyName("won")] [UsedImplicitly] public bool Won { get; set; } = won;
    [JsonPropertyName("prize_type")] [UsedImplicitly] public string PrizeType { get; set; } = prizeType;
    [JsonPropertyName("prize_value")] [UsedImplicitly] public string PrizeValue { get; set; } = prizeValue;
    [JsonPropertyName("blind")] [UsedImplicitly] public int Blind { get; set; } = blind;
}