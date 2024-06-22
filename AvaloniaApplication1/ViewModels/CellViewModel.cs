using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaApplication1.ViewModels;

public enum CellState
{
    Empty,
    X,
    O
}

public class CellViewModel : ViewModelBase
{
    private readonly GameWindowViewModel _gameWindowViewModel;

    public int Row { get; }

    public int Column { get; }

    [Reactive] public CellState State { get; set; }
    public ReactiveCommand<Unit, Unit> CellClickedCommand { get; }

    [Reactive] public bool IsBright { get; set; }

    public CellViewModel(GameWindowViewModel gameWindowViewModel, int row, int column,
        CellState state = CellState.Empty)
    {
        _gameWindowViewModel = gameWindowViewModel;
        Row = row;
        Column = column;
        State = state;

        CellClickedCommand = ReactiveCommand.Create(CellClick);
    }

    private void CellClick()
    {
        _gameWindowViewModel.CellClicked(Row, Column);
    }
}