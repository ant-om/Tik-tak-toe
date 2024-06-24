using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Models;

public static class NetworkingModel
{
    public static async Task<Socket> CreateClientSocket(string address)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(IPEndPoint.Parse(address));
        return socket;
    }

    private static EndPoint? GetLocalIp()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("1.1.1.1", 65535);
        return socket.LocalEndPoint;
    }

    public static Socket CreateListeningSocket()
    {
        var listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listeningSocket.Bind(GetLocalIp()!);
        listeningSocket.Listen();
        return listeningSocket;
    }

    public static void ReceiveMoves(Socket socket, Action<int> moveHandler)
    {
        var buffer = new byte[1024];
        Task.Run(async () =>
        {
            while (true)
            {
                var received = await socket.ReceiveAsync(buffer);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                foreach (var cmd in response)
                {
                    var cellId = cmd - '0';
                    moveHandler(cellId);
                }
            }
        }).ContinueWith(t =>
        {
            if (t.IsFaulted) throw t.Exception;
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}