using System.Text.Json;
using BalatroMultiplayer.Prizes;

namespace BalatroMultiplayer;

public class BlindData(int blind)
{
    private static readonly List<BlindData> ContestedBlinds = [];
    private class PlayerData(int score, bool complete)
    {
        public int Score = score;
        public bool Complete = complete;
    }
    private readonly Dictionary<Guid, PlayerData> _playerScores = new();
    private readonly int _blind = blind;

    public static void UpdateScore(Player player, int blind, int score)
    {
        var blindData = ContestedBlinds.Find(cb => cb._blind == blind);
        
        if (blindData is null)
        {
            blindData = new BlindData(blind);
            ContestedBlinds.Add(blindData);
        }
        
        blindData.UpdateScore(player, score);
    }

    public static void Reset()
    {
        ContestedBlinds.Clear();
    }

    public static void MarkCompletedFor(Player player, int blind)
    {
        ContestedBlinds.Find(cb => cb._blind == blind)?.OnPlayerComplete(player);
    }

    private void UpdateScore(Player player, int score)
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
            Task.Run(()=>Player.All()[0].SendMessage(new MessageContainer("last_on_blind", JsonSerializer.Serialize(_blind))));
        }
    }

    private void OnPlayerComplete(Player player)
    {
        _playerScores[player.Id].Complete = true;
        var completed = _playerScores.Count(p => p.Value.Complete);

        // if (Player.PlayerCount - completed == 1)
        // {
        //     Guid playerId = _playerScores.FirstOrDefault(pl => !pl.Value.Complete).Key;
        //
        //     Task.Run(() => 
        //         Player.GetById(playerId)?.SendMessage(
        //             new MessageContainer("last_on_blind", 
        //                 JsonSerializer.Serialize(_blind)
        //             )
        //         )
        //     );
        // }

        if (completed != Player.PlayerCount) return;
        ContestedBlinds.Remove(this);
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
        
        var prize = Prize.Prizes[1];

        WinLoseMessage winMessage = new(true, prize.Identifier, JsonSerializer.Serialize(prize.GetPrizeJson()), _blind);

        Task.Run(() => winningPlayer?.SendMessage(new MessageContainer(
            "declare_winner",
            winMessage
        )));
        
        var loseMessage = new WinLoseMessage(false, winMessage.PrizeType, winMessage.PrizeValue, _blind);
        foreach (var loser in Player.All().Where(pl => pl != winningPlayer))
        {
            Task.Run(() => loser.SendMessage(new MessageContainer(
                "declare_winner",
                loseMessage
            )));
        }

    }
}