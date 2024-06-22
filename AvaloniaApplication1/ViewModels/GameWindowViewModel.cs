using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Reactive;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;


namespace AvaloniaApplication1.ViewModels;

public class GameWindowViewModel : ViewModelBase
{
    public ObservableCollection<CellViewModel> Cells { get; }

    private CellState _currentPlayer = CellState.X;
    private Socket? _socket;

    public GameWindowViewModel()
    {
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

    public void CellClicked(int row, int column)
    {
        var cellIndex = row * 3 + column;
        if (Cells[cellIndex].State == CellState.Empty)
        {
            Cells[cellIndex].State = _currentPlayer;
            if (_currentPlayer == CellState.X)
            {
                _currentPlayer = CellState.O;
            }
            else
            {
                _currentPlayer = CellState.X;
            }
        }

        int[][] wins =
        [
            [0, 1, 2], [3, 4, 5], [6, 7, 8], [0, 3, 6], [1, 4, 7], [2, 5, 8], [2, 4, 6], [0, 4, 8]
        ];
        foreach (var win in wins)
        {
            if (Cells[win[0]].State == Cells[win[1]].State &&
                Cells[win[1]].State == Cells[win[2]].State)
            {
                var winner = Cells[win[0]].State;
                if (winner == CellState.Empty) continue;
                // Console.WriteLine($"{winner} won");
                // GameInProgress = false;
                foreach (var i in win)
                {
                    Cells[i].IsBright = true;
                    // Console.WriteLine($"{winner} won");
                    GameInProgress = false;
                }
            }
        }
    }

    [Reactive] public bool GameInProgress { get; set; } = true;
}