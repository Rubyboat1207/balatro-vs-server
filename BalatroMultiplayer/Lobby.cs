namespace BalatroMultiplayer;

public class Lobby(string id)
{
    private static readonly Dictionary<string, Lobby> Lobbies = [];
    public List<BlindData> ContestedBlinds { get; } = [];
    public SemaphoreSlim CurrentGameLock = new SemaphoreSlim(1, 1);
    public StartGameMessage? CurrentGame;
    public readonly List<Player> Players = [];

    private void Join(Player player)
    {
        if (player.LobbyId is not null)
        {
            GetById(player.LobbyId)?.Leave(player);
        }
        Players.Add(player);
        player.LobbyId = id;

        if (CurrentGame is not null)
        {
            Task.Run(() => player.SendMessage(new MessageContainer("start_game", CurrentGame)));
        }
    }

    public async void OnPlayerStartedGame(Player player, StartGameMessage game)
    {
        await CurrentGameLock.WaitAsync();

        if (CurrentGame is not null) return;

        CurrentGame = game;
        
        foreach (var others  in Players.Where(pl => pl != player))
        {
            await others.SendMessage(new MessageContainer("start_game", game));
        }

        CurrentGameLock.Release();
    }

    public void Leave(Player player)
    {
        Players.Remove(player);
        player.LobbyId = null;

        if (Players.Count != 0) return;
        Lobbies.Remove(id);
        ContestedBlinds.Clear();
    }

    public async Task WinCheck()
    {
        if (Players.Count(pl => !pl.LostGame) != 1) return;
        
        await Players.First(pl => !pl.LostGame).SendMessage(new MessageContainer("game_normal", null));
        CurrentGame = null;
    }

    public static Lobby? GetById(string? id)
    {
        return id == null ? null : Lobbies.GetValueOrDefault(id);
    }

    public static void Join(string id, Player player)
    {
        if (!Lobbies.TryGetValue(id, out var lobby))
        {
            lobby = new Lobby(id);
            Lobbies.Add(id, lobby);
        }
        
        lobby.Join(player);
    }
    
    public void UpdateScore(Player player, int blind, int score)
    {
        var blindData = ContestedBlinds.Find(cb => cb.Blind == blind);
        
        if (blindData is null)
        {
            blindData = new BlindData(this, blind);
            ContestedBlinds.Add(blindData);
        }
        
        blindData.UpdateScore(player, score);
    }
    
    public void MarkBlindCompletedFor(Player player, int blind)
    {
        ContestedBlinds.Find(cb => cb.Blind == blind)?.OnPlayerComplete(player);
    }
}