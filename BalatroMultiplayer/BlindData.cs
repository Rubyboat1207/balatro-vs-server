using System.Text.Json;
using BalatroMultiplayer.Prizes;

namespace BalatroMultiplayer;

public class BlindData(Lobby lobby, int blind)
{
    private class PlayerData(int score, bool complete)
    {
        public int Score = score;
        public bool Complete = complete;
    }
    private readonly Dictionary<Guid, PlayerData> _playerScores = new();
    public readonly int Blind = blind;

    public void UpdateScore(Player player, int score)
    {
        if (_playerScores.TryGetValue(player.Id, out var value))
        {
            value.Score = score;
        }
        else
        {
            _playerScores.Add(player.Id, new PlayerData(score, false));
        }

        if (Player.PlayerCount == 1)
        {
            Task.Run(()=>Player.All()[0].SendMessage(new MessageContainer("last_on_blind", JsonSerializer.Serialize(Blind))));
        }
    }

    public void OnPlayerComplete(Player player)
    {
        _playerScores[player.Id].Complete = true;
        var completed = _playerScores.Count(p => p.Value.Complete);
        
        if (completed != lobby.Players.Count) return;
        lobby.ContestedBlinds.Remove(this);
        KeyValuePair<Guid, PlayerData>? winner = null;

        foreach (var score in _playerScores)
        {
            if (winner == null)
            {
                winner = score;
                continue;
            }

            if (winner.Value.Value.Score < score.Value.Score)
            {
                winner = score;
            }
        }

        if (winner == null) return;
        
        var winningPlayer = Player.GetById(winner.Value.Key);

        int? hardcodedPrize = null;
        var prize = hardcodedPrize is null ? Prize.Prizes[new Random().Next(0, Prize.Prizes.Length)] : Prize.Prizes[hardcodedPrize.Value];

        WinLoseMessage winMessage = new(true, prize.Identifier, JsonSerializer.Serialize(prize.GetPrizeJson()), Blind);

        Task.Run(() => winningPlayer?.SendMessage(new MessageContainer(
            "declare_winner",
            winMessage
        )));
        
        var loseMessage = new WinLoseMessage(false, winMessage.PrizeType, winMessage.PrizeValue, Blind);
        foreach (var loser in lobby.Players.Where(pl => pl != winningPlayer))
        {
            Task.Run(() => loser.SendMessage(new MessageContainer(
                "declare_winner",
                loseMessage
            )));
        }

    }
}