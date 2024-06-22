using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaApplication1.ViewModels;

public enum CellState
{
    X,
    O
}

public class CellViewModel : ViewModelBase
{
    private readonly GameWindowViewModel _gameWindowViewModel;
    private readonly ObservableAsPropertyHelper<bool> _isHorizontal;
    private readonly ObservableAsPropertyHelper<bool> _isVertical;
    private readonly ObservableAsPropertyHelper<bool> _isDiagonal_l;
    private readonly ObservableAsPropertyHelper<bool> _isDiagonal_r;

    public CellViewModel(GameWindowViewModel gameWindowViewModel, int row, int column,
        CellState? state = null)
    {
        _gameWindowViewModel = gameWindowViewModel;
        Row = row;
        Column = column;
        State = state;

        CellClickedCommand = ReactiveCommand.Create(CellClick);

        var onKlass = this.WhenAnyValue(cell => cell.Klass);
        onKlass
            .Select(klass => klass == KlassType.Horizontal)
            .ToProperty(this, nameof(IsHorizontal), out _isHorizontal);
        onKlass
            .Select(klass => klass == KlassType.Vertical)
            .ToProperty(this, nameof(IsVertical), out _isVertical);
        onKlass
            .Select(klass => klass == KlassType.Diagonal_l)
            .ToProperty(this, nameof(IsDiagonal_l), out _isDiagonal_l);
        onKlass
            .Select(klass => klass == KlassType.Diagonal_r)
            .ToProperty(this, nameof(IsDiagonal_r), out _isDiagonal_r);
    }

    public int Row { get; }

    public int Column { get; }

    [Reactive] public CellState? State { get; set; }
    public ReactiveCommand<Unit, Unit> CellClickedCommand { get; }

    [Reactive] public KlassType? Klass { get; set; }
    public bool IsHorizontal => _isHorizontal.Value;
    public bool IsVertical => _isVertical.Value;
    public bool IsDiagonal_l => _isDiagonal_l.Value;
    public bool IsDiagonal_r => _isDiagonal_r.Value;

    private void CellClick()
    {
        _gameWindowViewModel.CellClicked(Row, Column);
    }
}

public enum KlassType
{
    Horizontal,
    Vertical,
    Diagonal_l,
    Diagonal_r
}