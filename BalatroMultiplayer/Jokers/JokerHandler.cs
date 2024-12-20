namespace BalatroMultiplayer.Jokers;

public abstract class JokerHandler
{
    public static readonly JokerHandler[] Handlers = [
        new SendMessageHandler("ghoulish_imp", 
            () => new MessageContainer("delete_card", new {kind = "playing_card"}),
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