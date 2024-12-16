using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BalatroMultiplayer;

class Program
{
    private static readonly List<TcpClient> ConnectedClients = new List<TcpClient>();
    private static readonly object ClientListLock = new object();

    static async Task Main(string[] args)
    {
        const int port = 5304; // Specify the port to listen on
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Server started on port {port}.");

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                lock (ClientListLock)
                {
                    ConnectedClients.Add(client);
                }

                // Handle the client in a separate task
                _ = Task.Run(() => HandleClientAsync(client));
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

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            await using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                
                var handler = JsonSerializer.Deserialize<MessageContainer>(receivedMessage)!.GetHandler();
                if (handler == null)
                {
                    throw new Exception($"Unknown Message! {receivedMessage}");
                }

                TcpClient[] clientsCopy;
                lock(ClientListLock)
                {
                    clientsCopy = ConnectedClients.ToArray();
                }

                await handler.Handle(clientsCopy, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred with a client: {ex.Message}");
        }
        finally
        {
            lock (ClientListLock)
            {
                ConnectedClients.Remove(client);
            }

            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}
