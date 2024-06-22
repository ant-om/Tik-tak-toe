using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;


namespace AvaloniaApplication1.ViewModels;

public class GameWindowViewModel : ViewModelBase
{
    public ObservableCollection<CellViewModel> Cells { get; }

    private readonly CellState _localPlayer;
    private CellState _currentPlayer = CellState.X;
    private readonly Socket _socket;
    private readonly byte[] _buffer = new byte[1024];

    public GameWindowViewModel(Socket socket, CellState localPlayer)
    {
        _socket = socket;
        _localPlayer = localPlayer;
        CanMove = _localPlayer == _currentPlayer;
        Task.Run(async () =>
        {
            while (true)
            {
                string response;
                // try
                // {
                var received = await socket.ReceiveAsync(_buffer);
                response = Encoding.UTF8.GetString(_buffer, 0, received);
                // }
                // catch (SocketException)
                // {
                //     break;
                // }
                // catch (ObjectDisposedException)
                // {
                //     break;
                // }

                foreach (var cmd in response)
                {
                    var cellId = cmd - '0';
                    var remotePlayer = _localPlayer == CellState.X ? CellState.O : CellState.X;
                    if (cellId is < 0 or > 8) continue;
                    HandleMove(cellId, remotePlayer);
                }
            }
        }).ContinueWith(t =>
        {
            if (t.IsFaulted) throw t.Exception;
        }, TaskScheduler.FromCurrentSynchronizationContext());
        ChoosePlayerCommand = ReactiveCommand.Create<string, Unit>(ChoosePlayer);
        Cells =
        [
            new(this, 0, 0), new(this, 0, 1), new(this, 0, 2),
            new(this, 1, 0), new(this, 1, 1), new(this, 1, 2),
            new(this, 2, 0), new(this, 2, 1), new(this, 2, 2),
        ];
    }

    public ReactiveCommand<string, Unit> ChoosePlayerCommand { get; }

    private Unit ChoosePlayer(string arg)
    {
        return Unit.Default;
    }

    private struct Win(int[] positions, KlassType klass)
    {
        public int[] Positions = positions;
        public KlassType Klass = klass;
    }

    public void CellClicked(int row, int column)
    {
        var cellIndex = row * 3 + column;
        if (!HandleMove(cellIndex, _localPlayer)) return;
        try
        {
            _socket.Send(Encoding.UTF8.GetBytes($"{cellIndex}"));
        }
        catch
        {
            DialogContent = "poshol nahui";
            IsDialogOpen = true;
        }
    }

    [Reactive] public bool CanMove { get; set; }

    private bool HandleMove(int cellIndex, CellState player)
    {
        var moved = Cells[cellIndex].State is null && player == _currentPlayer;
        if (moved)
        {
            Cells[cellIndex].State = player;
            _currentPlayer = _currentPlayer == CellState.X ? CellState.O : CellState.X;
        }

        Win[] wins =
        [
            new([0, 1, 2], KlassType.Horizontal), new([3, 4, 5], KlassType.Horizontal),
            new([6, 7, 8], KlassType.Horizontal),
            new([0, 3, 6], KlassType.Vertical), new([1, 4, 7], KlassType.Vertical),
            new([2, 5, 8], KlassType.Vertical), /*ver*/
            new([2, 4, 6], KlassType.Diagonal_l), new([0, 4, 8], KlassType.Diagonal_r)
        ];
        foreach (var win in wins)
        {
            if (Cells[win.Positions[0]].State == Cells[win.Positions[1]].State &&
                Cells[win.Positions[1]].State == Cells[win.Positions[2]].State)
            {
                var winner = Cells[win.Positions[0]].State;
                if (winner is null) continue;
                DialogContent = "ya tvoi hui shatal";
                IsDialogOpen = true;
                // Console.WriteLine($"{winner} won");
                // GameInProgress = false;
                foreach (var i in win.Positions)
                {
                    Cells[i].Klass = win.Klass;
                    // Console.WriteLine($"{winner} won");
                    GameInProgress = false;
                }
            }
        }

        if (Cells.All(cell => cell.State is not null) && GameInProgress)
        {
            GameInProgress = false;
            DialogContent = "sosi man pimpis";
            IsDialogOpen = true;
        }

        CanMove = GameInProgress && _localPlayer == _currentPlayer;
        return moved;
    }

    [Reactive] public bool GameInProgress { get; set; } = true;
    [Reactive] public bool IsDialogOpen { get; set; }
    [Reactive] public string DialogContent { get; set; }
}