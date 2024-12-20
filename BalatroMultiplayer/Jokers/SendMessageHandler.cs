namespace BalatroMultiplayer.Jokers;

public class SendMessageHandler(string ident, Func<MessageContainer> messageProducer, Func<Player, string?, Func<Player, int, bool>>? playerPredicate=null) : JokerHandler
{
    public override string Identifier => ident;
    public override async Task Handle(Player player, string? extraData)
    {
        var msg = messageProducer.Invoke();

        IEnumerable<Player> players = Player.All();

        if (playerPredicate is not null)
        {
            players = players.Where(playerPredicate.Invoke(player, extraData));
        }
        
        if(playerPredicate is null) await Task.WhenAll(players.Select(pl => pl.SendMessage(msg)));
    }
}