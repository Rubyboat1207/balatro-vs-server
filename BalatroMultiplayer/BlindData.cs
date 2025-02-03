using System.Text.Json;
using BalatroMultiplayer.Prizes;

namespace BalatroMultiplayer;

public class BlindData(Lobby lobby, int blind)
{
    private class PlayerData(double score, bool complete, DateTime? completionTime=null)
    {
        public double Score = score;
        public bool Complete = complete;
        public DateTime? CompletionTime = completionTime;
    }
    private readonly Dictionary<Guid, PlayerData> _playerScores = new();
    public readonly int Blind = blind;

    public void UpdateScore(Player player, double score)
    {
        if (_playerScores.TryGetValue(player.Id, out var value))
        {
            value.Score = score;
        }
        else
        {
            _playerScores.Add(player.Id, new PlayerData(score, false));
        }

        if (_playerScores.Where(kvp => kvp.Key != player.Id).Select(kvp => kvp.Value.Complete).Count(complete => complete) == lobby.Players.Count - 1)
        {
            _ = player.SendMessage(new MessageContainer("last_on_blind", JsonSerializer.Serialize(Blind)));
        }
    }

    public void OnPlayerComplete(Player player)
    {
        _playerScores[player.Id].Complete = true;
        _playerScores[player.Id].CompletionTime = DateTime.Now;
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
            
            // if scores are equal, check completion time
            if (!(Math.Abs(winner.Value.Value.Score - score.Value.Score) < 0.1)) continue;
            
            if (winner.Value.Value.CompletionTime > score.Value.CompletionTime)
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