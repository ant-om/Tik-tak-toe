using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaApplication1.ViewModels;

public class ConnectionMenuWindowViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<string> _hostButtonText;

    public ConnectionMenuWindowViewModel()
    {
        this.WhenAnyValue(vm => vm.ListeningSocket)
            .Select(socket => socket is null ? "HOST" : socket.LocalEndPoint!.ToString()!)
            .ToProperty(this, nameof(HostButtonText), out _hostButtonText);
    }

    [Reactive] private Socket? ListeningSocket { get; set; }
    public string HostButtonText => _hostButtonText.Value;

    public async void Host()
    {
        using var listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listeningSocket.Bind(GetLocalIp()!);
        listeningSocket.Listen();
        ListeningSocket = listeningSocket;
        var communicationSocket = await listeningSocket.AcceptAsync();
        await ShowGameWindow.Handle((communicationSocket, CellState.X));
    }

    public void OpenConnectionDialog() => IsDialogOpen = true;

    public async void Connect() => await Connect(ConnectionString);

    private async Task Connect(string address)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(IPEndPoint.Parse(address));
        await ShowGameWindow.Handle((socket, CellState.O));
    }

    private static EndPoint? GetLocalIp()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("1.1.1.1", 65535);
        return socket.LocalEndPoint;
    }

    public Interaction<(Socket, CellState), Unit> ShowGameWindow { get; } = new();
    [Reactive] public bool IsDialogOpen { get; set; }
    [Reactive] public string ConnectionString { get; set; } = "192.168.50.238:1234"; // TODO remove this
}