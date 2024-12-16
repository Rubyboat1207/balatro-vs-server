using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BalatroMultiplayer;

public interface IMessageHandler
{
    Task Handle(TcpClient[] clients, TcpClient sender);
}

public class MessageContainer(string type, object data)
{
    [JsonPropertyName("type")] private string Type { get; set; } = type;

    [JsonPropertyName("data")] private string Data { get; set; } = JsonSerializer.Serialize(data);

    public IMessageHandler? GetHandler()
    {
        if (Type == "hand_score")
        {
            return JsonSerializer.Deserialize<UpdateScoreMessage>(Data);
        }

        return null;
    }
}

public class UpdateScoreMessage(int score) : IMessageHandler
{
    [JsonPropertyName("score")]
    public int Score { get; init; } = score;

    public async Task Handle(TcpClient[] clients, TcpClient sender)
    {
        foreach (var client in clients.Where(client => client != sender))
        {
            await using var stream = client.GetStream();

            await stream.WriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new MessageContainer("update_score", this))));
        }
    }
}