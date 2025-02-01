using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BalatroMultiplayer;

public class Player
{
    private static readonly List<Player> ConnectedClients = [];
    private static readonly object ClientListLock = new();
    private readonly TcpClient _client;
    public bool LostGame = false;
    public Guid Id = Guid.NewGuid();
    public string? LobbyId = null;
    public Lobby? Lobby => Lobby.GetById(LobbyId);

    public Player(TcpClient client)
    {
        _client = client;
        ConnectedClients.Add(this);
    }

    public static Player? GetById(Guid id)
    {
        lock (ClientListLock)
        {
            return ConnectedClients.Find(p => p.Id == id);
        }
    }
    
    // Dont use generally.
    public static Player[] All()
    {
        Player[] players;

        lock (ClientListLock)
        {
            players = ConnectedClients.ToArray();
        }

        return players;
    }

    public static int PlayerCount
    {
        get
        {
            lock (ClientListLock)
            {
                return ConnectedClients.Count;
            }
        }
    }

    private void OnLeave()
    {
        ConnectedClients.Remove(this);

        if (LobbyId != null) Lobby.GetById(LobbyId)?.Leave(this);
    }

    public async void BeginListening()
    {
        try
        {
            NetworkStream stream = _client.GetStream();
            var buffer = new byte[1024];

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedMessage);
                
                var handler = JsonSerializer.Deserialize<MessageContainer>(receivedMessage)!.GetHandler();
                if (handler == null)
                {
                    Console.WriteLine($"Unknown Message! {receivedMessage}");
                    continue;
                }

                var clients = LobbyId is not null ? Lobby.GetById(LobbyId)!.Players.ToArray() : [];

                await handler.Handle(clients, this);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred with a client: {ex}");
        }
        finally
        {
            lock (ClientListLock)
            {
                OnLeave();
            }

            _client.Close();
            Console.WriteLine("Client disconnected.");
        }
    } 

    public async Task SendMessage(MessageContainer message)
    {
        if (!_client.Connected)
        {
            Console.WriteLine($"Somewhere in the code has attempted to send a message to a since disconnected client. Message was: '{JsonSerializer.Serialize(message)}'");
            return;
        }

        string msg = JsonSerializer.Serialize(message);
        Console.WriteLine($"Sending: {msg}");
        await _client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(msg + "\n"));
    }
}