using System.Text.Json.Serialization;

namespace BalatroMultiplayer.Abilities;

public abstract class AbilityHandler
{
    public static readonly AbilityHandler[] Handlers = [
        new SendMessageHandler("force_select_single", // ghoulish imp
            () => new MessageContainer("hand_effect", new HandEffect("hand", "random", "force_select")),
            PlayerPredicates.NotSender
        ),
        new SendMessageHandler("flip_all", // old mask tarot
            () => new MessageContainer("hand_effect", new HandEffect("hand", "all", "flip")),
            PlayerPredicates.NotSender
        ),
        new SendMessageHandler("flip_half", // mask tarot
            () => new MessageContainer("hand_effect", new HandEffect("hand", "half", "flip")),
            PlayerPredicates.NotSender
        ),
        new SendMessageHandler("remove_edition", // square of death effect
            () => new MessageContainer("hand_effect", new HandEffect("jokers", "random_with_edition", "reset_edition"))
        )
    ];
    public abstract string Identifier { get; }
    public abstract Task Handle(Player player, string? extraData);

    private static class PlayerPredicates
    {
        public static readonly Func<Player, string?, Func<Player, int, bool>> NotSender = (sender, _) =>
            (player, _) => player != sender;
    }
    
    private class HandEffect(string cardArea, string selectorType, string effect)
    {
        [JsonPropertyName("area")]
        public string CardArea { get; set; } = cardArea;
        [JsonPropertyName("selector")]
        public string SelectorType { get; set; } = selectorType;
        [JsonPropertyName("effect")]
        public string Effect { get; set; } = effect;
    }
}