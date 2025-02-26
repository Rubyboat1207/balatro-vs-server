﻿namespace BalatroMultiplayer.Abilities;

public class SendMessageHandler(string ident, Func<MessageContainer> messageProducer, Func<Player, string?, Func<Player, int, bool>>? playerPredicate=null) : AbilityHandler
{
    public override string Identifier => ident;
    public override async Task Handle(Player player, string? extraData)
    {
        var msg = messageProducer.Invoke();
        var lobby = player.Lobby;

        if (lobby is null) return;

        IEnumerable<Player> players = lobby.Players;

        if (playerPredicate is not null)
        {
            players = players.Where(playerPredicate.Invoke(player, extraData));
        }

        await Task.WhenAll(players.Select(pl => pl.SendMessage(msg)));
    }
}