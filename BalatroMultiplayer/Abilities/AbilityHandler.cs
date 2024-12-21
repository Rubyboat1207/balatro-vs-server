namespace BalatroMultiplayer.Abilities;

public abstract class AbilityHandler
{
    public static readonly AbilityHandler[] Handlers = [
        new SendMessageHandler("force_select_single", // ghoulish imp
            () => new MessageContainer("hand_effect", new {effect = "force_select", selector = "random"}),
            PlayerPredicates.NotSender
        ),
        new SendMessageHandler("flip_all", // old mask tarot
            () => new MessageContainer("hand_effect", new {effect = "flip", selector = "all"}),
            PlayerPredicates.NotSender
        ),
        new SendMessageHandler("flip_half", // mask tarot
            () => new MessageContainer("hand_effect", new {effect = "flip", selector = "half"}),
            PlayerPredicates.NotSender
        ),
    ];
    public abstract string Identifier { get; }
    public abstract Task Handle(Player player, string? extraData);

    private static class PlayerPredicates
    {
        public static readonly Func<Player, string?, Func<Player, int, bool>> NotSender = (sender, _) =>
            (player, _) => player != sender;
    }
}