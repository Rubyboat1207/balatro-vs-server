using System.Net;
using System.Net.Sockets;

namespace BalatroMultiplayer;

internal static class Program
{
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
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                _ = Task.Run(() => new Player(client).BeginListening());
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
}