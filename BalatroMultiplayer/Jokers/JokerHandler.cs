namespace BalatroMultiplayer.Jokers;

public abstract class JokerHandler
{
    public static readonly JokerHandler[] Handlers = [
        new SendMessageHandler("ghoulish_imp", 
            () => new MessageContainer("hand_effect", new {effect = "force_select", selector = "random"}),
            PlayerPredicates.NotSender
        )
    ];
    public abstract string Identifier { get; }
    public abstract Task Handle(Player player, string? extraData);

    private static class PlayerPredicates
    {
        public static readonly Func<Player, string?, Func<Player, int, bool>> NotSender = (sender, _) =>
            (player, _) => player != sender;
    }
}