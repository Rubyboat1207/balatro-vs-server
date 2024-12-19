using System.Net;
using System.Net.Sockets;
using BalatroMultiplayer.Prizes;

namespace BalatroMultiplayer;

internal static class Program
{
    public static StartGameMessage? CurrentGame;
    private static async Task Main(string[] args)
    {
        const int port = 5304; // Specify the port to listen on
        var listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Server started on port {port}.");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                var player = new Player(client);
                Console.WriteLine($"Client ({(client.Client.RemoteEndPoint as IPEndPoint)?.Address}) connected");

                _ = Task.Run(() => player.BeginListening());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    public static void OnAllClientsDisconnected()
    {
        CurrentGame = null;
        BlindData.Reset();
    }
}